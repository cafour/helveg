#include "world_render.hpp"
#include "log.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>
#include <iostream>
#include <sstream>
#include <tuple>
#include <vector>

// If you change this, don't forget about fire.comp as well.
const size_t emitterParticleCount = 32 * 32;
const glm::vec4 skyColor = glm::vec4(0.533f, 0.808f, 0.925f, 1.0f);

static VkPhysicalDeviceFeatures getRequiredFeatures()
{
    VkPhysicalDeviceFeatures features = {};
    features.geometryShader = VK_TRUE;
    return features;
}

static const vku::Light sun = vku::Light {
    glm::vec4(-1.0f, -1.0f, -1.0f, 0.0f),
    glm::vec4(0.4f, 0.4f, 0.4f, 0.0f),
    glm::vec4(1.0f, 1.0f, 1.0f, 0.0f),
    glm::vec4(0.7f, 0.7f, 0.7f, 10.0f),
};

static const VkPhysicalDeviceFeatures requiredFeatures = getRequiredFeatures();

vku::WorldRender::WorldRender(int width, int height, World world, const std::string &title, bool debug)
    : _instanceCore("WorldRender", true, debug)
    , _displayCore(_instanceCore.instance(), width, height, title, &requiredFeatures)
    , _swapchainCore(_displayCore)
    , _renderCore(
          _displayCore,
          _swapchainCore,
          [this](auto &f) { return createFramebuffer(f); },
          [this](auto cb, auto &f) { recordCommandBuffer(cb, f); })
    , _cameraCore(_displayCore, _renderCore)
    , _depthCore(_displayCore, _renderCore)
    , _transferCore(_displayCore.physicalDevice(), _displayCore.device())
    , _world(world)
{
    VkPhysicalDeviceProperties properties;
    vkGetPhysicalDeviceProperties(_displayCore.physicalDevice(), &properties);
    std::stringstream ss;
    ss << "Using device '" << properties.deviceName << "'.";
    vku::logDebug(ss.str());

    _cameraCore.view().position = glm::vec3(0.0f, 128.0f, 0.0f);

    _renderCore.onResize([this](auto s, auto e) { onResize(s, e); });
    _renderCore.onUpdate([this](auto &f) { onUpdate(f); });

    _renderPass = vku::RenderPass::basic(
        _displayCore.device(),
        _displayCore.surfaceFormat().format,
        _depthCore.depthFormat());

    _timeBuffer = vku::Buffer::exclusive(
        _displayCore.device(),
        sizeof(vku::Time),
        VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    _timeMemory = vku::DeviceMemory::hostCoherentBuffer(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        _timeBuffer);

    createMeshes();
    createWorldGP();
    createFireGP();
    createFireCP();
}

void vku::WorldRender::createMeshes()
{
    logInformation("Creating meshes.");
    for (size_t i = 0; i < _world.chunkCount; ++i) {
        auto maybeMesh = vku::MeshCore::fromChunk(_transferCore, _world.chunks[i]);
        if (maybeMesh.has_value())
        {
            _meshes.push_back(std::move(maybeMesh.value()));
        }

        float chunkSize = static_cast<float>(_world.chunks[i].size);
        glm::vec3 chunkPosition = _world.chunkOffsets[i];
        _boxMax.x = std::max(_boxMax.x, chunkPosition.x + chunkSize);
        _boxMax.y = std::max(_boxMax.y, chunkPosition.y + chunkSize);
        _boxMax.z = std::max(_boxMax.z, chunkPosition.z + chunkSize);

        _boxMin.x = std::min(_boxMin.x, chunkPosition.x);
        _boxMin.y = std::min(_boxMin.y, chunkPosition.y);
        _boxMin.z = std::min(_boxMin.z, chunkPosition.z);
    }
}

void vku::WorldRender::createWorldGP()
{
    auto bindings = {
        vku::descriptorBinding(
            0,
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            1,
            VK_SHADER_STAGE_VERTEX_BIT),
        vku::descriptorBinding(
            1,
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            1,
            VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT)
    };

    _worldDSL = vku::DescriptorSetLayout::basic(_displayCore.device(), bindings.begin(), bindings.size());
    auto setLayouts = { _worldDSL.raw() };

    auto pushConstantRanges = {
        VkPushConstantRange { VK_SHADER_STAGE_VERTEX_BIT, 0, sizeof(glm::vec4) }, // vec3 is vec4 (see 14.5.4.)
        VkPushConstantRange { VK_SHADER_STAGE_FRAGMENT_BIT, sizeof(glm::vec4), sizeof(vku::Light) },
        VkPushConstantRange { VK_SHADER_STAGE_GEOMETRY_BIT, sizeof(glm::vec4) + sizeof(vku::Light), sizeof(float) }
    };

    _worldPL = vku::PipelineLayout::basic(
        _displayCore.device(),
        setLayouts.begin(),
        setLayouts.size(),
        pushConstantRanges.begin(),
        pushConstantRanges.size());

    auto vertexBindings = {
        vku::vertexInputBinding(0, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
        vku::vertexInputBinding(1, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
    };

    auto vertexAttributes = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 1, VK_FORMAT_R32G32B32_SFLOAT, 0)
    };

    auto vertexShader = vku::ShaderModule::inlined(_displayCore.device(), WORLD_VERT, WORLD_VERT_LENGTH);
    auto fragmentShader = vku::ShaderModule::inlined(_displayCore.device(), WORLD_FRAG, WORLD_FRAG_LENGTH);
    auto shaderStages = {
        vku::shaderStage(VK_SHADER_STAGE_VERTEX_BIT, vertexShader),
        vku::shaderStage(VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader)
    };

    _worldGP = vku::GraphicsPipeline::basic(
        _displayCore.device(),
        _worldPL,
        _renderPass,
        shaderStages.begin(),
        shaderStages.size(),
        vertexBindings.begin(),
        vertexBindings.size(),
        vertexAttributes.begin(),
        vertexAttributes.size(),
        VK_FRONT_FACE_COUNTER_CLOCKWISE);
}

void vku::WorldRender::createFireGP()
{
    if (_world.fireCount == 0) {
        return;
    }

    auto vertexShader = vku::ShaderModule::inlined(_displayCore.device(), FIRE_VERT, FIRE_VERT_LENGTH);
    auto geometryShader = vku::ShaderModule::inlined(_displayCore.device(), FIRE_GEOM, FIRE_GEOM_LENGTH);
    auto fragmentShader = vku::ShaderModule::inlined(_displayCore.device(), FIRE_FRAG, FIRE_FRAG_LENGTH);
    auto shaderStages = {
        vku::shaderStage(VK_SHADER_STAGE_VERTEX_BIT, vertexShader),
        vku::shaderStage(VK_SHADER_STAGE_GEOMETRY_BIT, geometryShader),
        vku::shaderStage(VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader)
    };

    auto vertexBindings = {
        vku::vertexInputBinding(0, sizeof(vku::Particle), VK_VERTEX_INPUT_RATE_VERTEX),
    };

    auto vertexAttributes = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 0, VK_FORMAT_R32G32B32A32_SFLOAT, sizeof(glm::vec4))
    };

    _fireGP = vku::GraphicsPipeline::basic(
        _displayCore.device(),
        _worldPL,
        _renderPass,
        shaderStages.begin(),
        shaderStages.size(),
        vertexBindings.begin(),
        vertexBindings.size(),
        vertexAttributes.begin(),
        vertexAttributes.size(),
        VK_FRONT_FACE_COUNTER_CLOCKWISE,
        VK_PRIMITIVE_TOPOLOGY_POINT_LIST);
}

void vku::WorldRender::createFireCP()
{
    if (_world.fireCount == 0) {
        return;
    }

    _emitterBuffer = vku::Buffer::exclusive(
        _displayCore.device(),
        sizeof(vku::Emitter) * _world.fireCount,
        VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    _emitterMemory = vku::DeviceMemory::deviceLocalData(
        _transferCore.physicalDevice(),
        _transferCore.device(),
        _transferCore.transferPool(),
        _transferCore.transferQueue(),
        _emitterBuffer,
        _world.fires,
        _world.fireCount * sizeof(vku::Emitter));

    _particleBuffer = vku::Buffer::exclusive(
        _displayCore.device(),
        sizeof(vku::Particle) * _world.fireCount * emitterParticleCount,
        VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    _particleMemory = vku::DeviceMemory::deviceLocalBuffer(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        _particleBuffer);
    vku::fillBuffer(
        _transferCore.device(),
        _transferCore.transferPool(),
        _transferCore.transferQueue(),
        _particleBuffer,
        VK_WHOLE_SIZE,
        0u);

    auto bindings = {
        vku::descriptorBinding(0, VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 1, VK_SHADER_STAGE_COMPUTE_BIT),
        vku::descriptorBinding(1, VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 1, VK_SHADER_STAGE_COMPUTE_BIT),
        vku::descriptorBinding(2, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_COMPUTE_BIT),
    };
    _fireDSL = vku::DescriptorSetLayout::basic(_displayCore.device(), bindings.begin(), bindings.size());
    auto setLayouts = { _fireDSL.raw() };

    auto poolSizes = {
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_STORAGE_BUFFER, 2),
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1)
    };
    _fireDP = vku::DescriptorPool::basic(_displayCore.device(), 1, poolSizes.begin(), poolSizes.size());
    _fireDS = vku::allocateDescriptorSets(_displayCore.device(), _fireDP, _fireDSL, 1)[0];
    vku::writeWholeBufferDescriptor(
        _displayCore.device(),
        VK_DESCRIPTOR_TYPE_STORAGE_BUFFER,
        _particleBuffer,
        _fireDS,
        0);
    vku::writeWholeBufferDescriptor(
        _displayCore.device(),
        VK_DESCRIPTOR_TYPE_STORAGE_BUFFER,
        _emitterBuffer,
        _fireDS,
        1);
    vku::writeWholeBufferDescriptor(
        _displayCore.device(),
        VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
        _timeBuffer,
        _fireDS,
        2);

    _firePL = vku::PipelineLayout::basic(
        _displayCore.device(),
        setLayouts.begin(),
        setLayouts.size());

    auto computeShader = vku::ShaderModule::inlined(_displayCore.device(), FIRE_COMP, FIRE_COMP_LENGTH);

    VkComputePipelineCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_COMPUTE_PIPELINE_CREATE_INFO;
    createInfo.layout = _firePL;
    createInfo.stage = vku::shaderStage(VK_SHADER_STAGE_COMPUTE_BIT, computeShader);

    _fireCP = vku::ComputePipeline(_displayCore.device(), createInfo);
}

void vku::WorldRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    ENSURE(vkBeginCommandBuffer(commandBuffer, &beginInfo));

    if (_world.fireCount > 0) {
        vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_COMPUTE, _fireCP);
        vkCmdBindDescriptorSets(
            commandBuffer,
            VK_PIPELINE_BIND_POINT_COMPUTE,
            _firePL,
            0, // first set
            1, // set count
            &_fireDS,
            0,
            nullptr);
        vkCmdDispatch(commandBuffer, _world.fireCount, 1, 1);
    }

    VkRenderPassBeginInfo renderPassInfo = {};
    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = _renderPass;
    renderPassInfo.framebuffer = _renderCore.framebuffers()[frame.index];
    renderPassInfo.renderArea.offset = { 0, 0 };

    VkExtent2D extent = _swapchainCore.extent();
    renderPassInfo.renderArea.extent = extent;

    VkClearValue clearValues[2];
    clearValues[0].color = { skyColor.r, skyColor.g, skyColor.b, skyColor.a };
    clearValues[1].depthStencil = { 1.0f, 0 };
    renderPassInfo.clearValueCount = 2;
    renderPassInfo.pClearValues = clearValues;

    VkViewport viewport = {};
    viewport.width = static_cast<float>(extent.width);
    viewport.height = static_cast<float>(extent.height);
    viewport.maxDepth = 1.0f;

    VkRect2D scissor = {};
    scissor.extent = extent;

    vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, _worldGP);
    vkCmdSetViewport(commandBuffer, 0, 1, &viewport);
    vkCmdSetScissor(commandBuffer, 0, 1, &scissor);

    vkCmdBindDescriptorSets(
        commandBuffer,
        VK_PIPELINE_BIND_POINT_GRAPHICS,
        _worldPL,
        0, // first set number
        1, // setCount
        &_worldDSs[frame.index],
        0, // dynamic offset count
        nullptr); // dynamic offsets
    vkCmdPushConstants(
        commandBuffer,
        _worldPL,
        VK_SHADER_STAGE_FRAGMENT_BIT,
        sizeof(glm::vec4), // offset
        sizeof(vku::Light), // size
        &sun);
    float aspect = viewport.width / viewport.height;
    vkCmdPushConstants(
        commandBuffer,
        _worldPL,
        VK_SHADER_STAGE_GEOMETRY_BIT,
        sizeof(glm::vec4) + sizeof(vku::Light), // offset
        sizeof(float), // size
        &aspect);
    for (size_t i = 0; i < _meshes.size(); ++i) {
        auto &mesh = _meshes[i];
        std::initializer_list<VkDeviceSize> offsets = { 0, mesh.vertexCount() * sizeof(glm::vec3) };
        auto vertexBuffers = { mesh.vertexBuffer().raw(), mesh.vertexBuffer().raw() };
        vkCmdBindVertexBuffers(commandBuffer, 0, vertexBuffers.size(), vertexBuffers.begin(), offsets.begin());
        vkCmdBindIndexBuffer(commandBuffer, mesh.indexBuffer(), 0, VK_INDEX_TYPE_UINT32);
        vkCmdPushConstants(
            commandBuffer,
            _worldPL,
            VK_SHADER_STAGE_VERTEX_BIT,
            0, // offset
            sizeof(glm::vec3), // size
            &_world.chunkOffsets[i]);
        vkCmdDrawIndexed(commandBuffer, static_cast<uint32_t>(mesh.indexCount()), 1, 0, 0, 0);
    }

    if (_world.fireCount > 0) {
        vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, _fireGP);
        VkDeviceSize particlesOffset = 0;
        vkCmdBindVertexBuffers(commandBuffer, 0, 1, _particleBuffer, &particlesOffset);
        vkCmdDraw(commandBuffer, _world.fireCount * emitterParticleCount, 1, 0, 0);
    }

    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

vku::Framebuffer vku::WorldRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    auto extent = _swapchainCore.extent();
    VkImageView attachments[] = {
        frame.imageView,
        _depthCore.depthImageView()
    };
    return vku::Framebuffer::basic(
        _displayCore.device(),
        _renderPass,
        attachments,
        2,
        extent.width,
        extent.height);
}

void vku::WorldRender::onResize(size_t imageCount, VkExtent2D)
{
    // recreate the uniform buffers as the number of swapchain images could have changed
    _uboBufferMemories.clear();
    _uboBuffers.clear();

    _uboBuffers.resize(imageCount);
    _uboBufferMemories.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        _uboBuffers[i] = vku::Buffer::exclusive(
            _displayCore.device(),
            sizeof(glm::mat4),
            VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
        _uboBufferMemories[i] = vku::DeviceMemory::hostCoherentBuffer(
            _displayCore.physicalDevice(),
            _displayCore.device(),
            _uboBuffers[i]);
    }

    VkDescriptorPoolSize poolSizes[] = {
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 2 * imageCount)
    };
    _worldDP = vku::DescriptorPool::basic(_displayCore.device(), imageCount, poolSizes, 1);
    _worldDSs = vku::allocateDescriptorSets(_displayCore.device(), _worldDP, _worldDSL, imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        vku::writeWholeBufferDescriptor(
            _displayCore.device(),
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            _uboBuffers[i],
            _worldDSs[i],
            0);
        vku::writeWholeBufferDescriptor(
            _displayCore.device(),
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            _cameraCore.cameraBuffers()[i],
            _worldDSs[i],
            1);
    }
}

void vku::WorldRender::onUpdate(vku::SwapchainFrame &frame)
{
    glm::mat4 model = glm::identity<glm::mat4>();

    vku::hostDeviceCopy(_displayCore.device(), &model, _uboBufferMemories[frame.index], sizeof(glm::mat4));

    float currentTime = static_cast<float>(glfwGetTime());
    _time.secDelta = currentTime - _time.sec;
    _time.sec = currentTime;
    vku::hostDeviceCopy(_displayCore.device(), &_time, _timeMemory, sizeof(vku::Time));
}
