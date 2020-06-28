#include "world_render.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>
#include <iostream>
#include <tuple>
#include <vector>

static VkPhysicalDeviceFeatures getRequiredFeatures()
{
    VkPhysicalDeviceFeatures features = {};
    return features;
}

static const vku::Light sun = vku::Light {
    glm::vec4(-1.0f, -1.0f, -1.0f, 0.0f),
    glm::vec4(1.0f, 1.0f, 1.0f, 0.0f),
    glm::vec4(1.0f, 1.0f, 1.0f, 0.0f),
    glm::vec4(1.0f, 1.0f, 1.0f, 0.0f),
};

static const VkPhysicalDeviceFeatures requiredFeatures = getRequiredFeatures();

vku::WorldRender::WorldRender(int width, int height, World world, bool debug)
    : _instanceCore("WorldRender", true, debug)
    , _displayCore(_instanceCore.instance(), width, height, "vkdev", &requiredFeatures)
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
    std::cout << "Device name: " << properties.deviceName << std::endl;

    for (size_t i = 0; i < world.count; ++i) {
        _meshes.push_back(vku::MeshCore::fromChunk(_transferCore, _world.chunks[i]));

        float chunkSize = static_cast<float>(world.chunks[i].size);
        glm::vec3 chunkPosition = world.positions[i];
        _boxMax.x = std::max(_boxMax.x, chunkPosition.x + chunkSize);
        _boxMax.y = std::max(_boxMax.y, chunkPosition.y + chunkSize);
        _boxMax.z = std::max(_boxMax.z, chunkPosition.z + chunkSize);

        _boxMin.x = std::min(_boxMin.x, chunkPosition.x);
        _boxMin.y = std::min(_boxMin.y, chunkPosition.y);
        _boxMin.z = std::min(_boxMin.z, chunkPosition.z);
    }

    VkDescriptorSetLayoutBinding bindings[] = {
        vku::descriptorBinding(0, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_VERTEX_BIT),
        vku::descriptorBinding(1, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT)
    };
    _setLayout = vku::DescriptorSetLayout::basic(_displayCore.device(), bindings, 2);
    std::vector<VkDescriptorSetLayout> setLayouts { _setLayout };

    _renderPass = vku::RenderPass::basic(
        _displayCore.device(),
        _displayCore.surfaceFormat().format,
        _depthCore.depthFormat());

    std::vector<VkPushConstantRange> pushConstantRanges {
        VkPushConstantRange { VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT, 0, sizeof(glm::vec3) },
        VkPushConstantRange { VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT, sizeof(glm::vec3), sizeof(vku::Light) }
    };

    _pipelineLayout = vku::PipelineLayout::basic(_displayCore.device(), &setLayouts, &pushConstantRanges);

    VkVertexInputBindingDescription vertexBindings[] = {
        vku::vertexInputBinding(0, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
        vku::vertexInputBinding(1, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
    };

    VkVertexInputAttributeDescription vertexAttributes[] = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 1, VK_FORMAT_R32G32B32_SFLOAT, 0)
    };

    _pipeline = vku::GraphicsPipeline::basic(
        _displayCore.device(),
        _pipelineLayout,
        _renderPass,
        vku::ShaderModule::inlined(_displayCore.device(), WORLD_VERT, WORLD_VERT_LENGTH),
        vku::ShaderModule::inlined(_displayCore.device(), WORLD_FRAG, WORLD_FRAG_LENGTH),
        vertexBindings,
        2,
        vertexAttributes,
        2,
        VK_FRONT_FACE_COUNTER_CLOCKWISE);

    _renderCore.onResize([this](auto s, auto e) { onResize(s, e); });
    _renderCore.onUpdate([this](auto &f) { onUpdate(f); });
}

void vku::WorldRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    ENSURE(vkBeginCommandBuffer(commandBuffer, &beginInfo));

    VkRenderPassBeginInfo renderPassInfo = {};
    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = _renderPass;
    renderPassInfo.framebuffer = _renderCore.framebuffers()[frame.index];
    renderPassInfo.renderArea.offset = { 0, 0 };

    VkExtent2D extent = _swapchainCore.extent();
    renderPassInfo.renderArea.extent = extent;

    VkClearValue clearValues[2];
    clearValues[0].color = { 0.0f, 0.0f, 0.0f, 1.0f };
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
    vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, _pipeline);
    vkCmdSetViewport(commandBuffer, 0, 1, &viewport);
    vkCmdSetScissor(commandBuffer, 0, 1, &scissor);

    vkCmdBindDescriptorSets(
        commandBuffer,
        VK_PIPELINE_BIND_POINT_GRAPHICS,
        _pipelineLayout,
        0, // first set number
        1, // setCount
        &_descriptorSets[frame.index],
        0, // dynamic offset count
        nullptr); // dynamic offsets
    vkCmdPushConstants(
        commandBuffer,
        _pipelineLayout,
        VK_SHADER_STAGE_FRAGMENT_BIT,
        sizeof(glm::vec3), // offset
        sizeof(vku::Light), // size
        &sun);
    for (size_t i = 0; i < _meshes.size(); ++i) {
        auto &mesh = _meshes[i];
        VkDeviceSize offsets[] = { 0, mesh.vertexCount() * sizeof(glm::vec3) };
        VkBuffer vertexBuffers[] { mesh.vertexBuffer(), mesh.vertexBuffer() };
        vkCmdBindVertexBuffers(commandBuffer, 0, 2, vertexBuffers, offsets);
        vkCmdBindIndexBuffer(commandBuffer, mesh.indexBuffer(), 0, VK_INDEX_TYPE_UINT32);
        vkCmdPushConstants(
            commandBuffer,
            _pipelineLayout,
            VK_SHADER_STAGE_VERTEX_BIT,
            0, // offset
            sizeof(glm::vec3), // size
            &_world.positions[i]);
        vkCmdDrawIndexed(commandBuffer, static_cast<uint32_t>(mesh.indexCount()), 1, 0, 0, 0);
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
    _descriptorPool = vku::DescriptorPool::basic(_displayCore.device(), imageCount, poolSizes, 1);
    _descriptorSets = vku::allocateDescriptorSets(_displayCore.device(), _descriptorPool, _setLayout, imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        vku::writeWholeBufferDescriptor(
            _displayCore.device(),
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            _uboBuffers[i],
            _descriptorSets[i],
            0);
        vku::writeWholeBufferDescriptor(
            _displayCore.device(),
            VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER,
            _cameraCore.cameraBuffers()[i],
            _descriptorSets[i],
            1);
    }
}

void vku::WorldRender::onUpdate(vku::SwapchainFrame &frame)
{
    glm::vec3 worldSize = _boxMax - _boxMin;
    float scale = 1.0f / std::max({ worldSize.x, worldSize.y, worldSize.z });
    glm::vec3 offset = (_boxMax + _boxMin) / -2.0f;

    glm::mat4 model = glm::identity<glm::mat4>();
    model = glm::scale(model, glm::vec3(scale));
    model = glm::translate(model, offset);

    vku::hostDeviceCopy(_displayCore.device(), &model, _uboBufferMemories[frame.index], sizeof(glm::mat4));
}
