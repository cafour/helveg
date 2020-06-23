#include "chunk_render.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>
#include <tuple>
#include <vector>

static VkPhysicalDeviceFeatures getRequiredFeatures()
{
    VkPhysicalDeviceFeatures features = {};
    return features;
}

static const VkPhysicalDeviceFeatures requiredFeatures = getRequiredFeatures();

vku::ChunkRender::ChunkRender(int width, int height, Chunk chunk, bool debug)
    : _instanceCore("ChunkRender", true, debug)
    , _displayCore(_instanceCore.instance(), width, height, "vkdev", &requiredFeatures)
    , _swapchainCore(_displayCore)
    , _renderCore(
          _displayCore,
          _swapchainCore,
          [this](auto &f) { return createFramebuffer(f); },
          [this](auto cb, auto &f) { recordCommandBuffer(cb, f); })
    , _depthCore(_displayCore, _renderCore)
    , _transferCore(_displayCore.physicalDevice(), _displayCore.device())
    , _meshCore(vku::MeshCore::fromChunk(_transferCore, chunk))
    , _chunk(chunk)
{
    VkDescriptorSetLayoutBinding bindings[] = {
        vku::descriptorBinding(0, VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, 1, VK_SHADER_STAGE_VERTEX_BIT)
    };
    _setLayout = vku::DescriptorSetLayout::basic(_displayCore.device(), bindings, 1);
    std::vector<VkDescriptorSetLayout> setLayouts { _setLayout };

    _renderPass = vku::RenderPass::basic(
        _displayCore.device(),
        _displayCore.surfaceFormat().format,
        _depthCore.depthFormat());

    _pipelineLayout = vku::PipelineLayout::basic(_displayCore.device(), &setLayouts);

    VkVertexInputBindingDescription vertexBindings[] = {
        vku::vertexInputBinding(0, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX),
        vku::vertexInputBinding(1, sizeof(glm::vec3), VK_VERTEX_INPUT_RATE_VERTEX)
    };

    VkVertexInputAttributeDescription vertexAttributes[] = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32B32_SFLOAT, 0),
        vku::vertexInputAttribute(1, 1, VK_FORMAT_R32G32B32_SFLOAT, 0)
    };

    _pipeline = vku::GraphicsPipeline::basic(
        _displayCore.device(),
        _pipelineLayout,
        _renderPass,
        vku::ShaderModule::inlined(_displayCore.device(), CHUNK_VERT, CHUNK_VERT_LENGTH),
        vku::ShaderModule::inlined(_displayCore.device(), CHUNK_FRAG, CHUNK_FRAG_LENGTH),
        vertexBindings,
        2,
        vertexAttributes,
        2,
        VK_FRONT_FACE_COUNTER_CLOCKWISE);

    _renderCore.onResize([this](auto s, auto e) { onResize(s, e); });
    _renderCore.onUpdate([this](auto &f) { onUpdate(f); });
}

void vku::ChunkRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
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
    viewport.width = extent.width;
    viewport.height = extent.height;
    viewport.maxDepth = 1.0f;

    VkRect2D scissor = {};
    scissor.extent = extent;

    vkCmdBeginRenderPass(commandBuffer, &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
    vkCmdBindPipeline(commandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, _pipeline);
    vkCmdSetViewport(commandBuffer, 0, 1, &viewport);
    vkCmdSetScissor(commandBuffer, 0, 1, &scissor);

    VkDeviceSize offsets[] = { 0, _meshCore.vertexCount() * sizeof(glm::vec3) };
    VkBuffer vertexBuffers[] { _meshCore.vertexBuffer(), _meshCore.vertexBuffer() };
    vkCmdBindVertexBuffers(commandBuffer, 0, 2, vertexBuffers, offsets);
    vkCmdBindIndexBuffer(commandBuffer, _meshCore.indexBuffer(), 0, VK_INDEX_TYPE_UINT32);
    vkCmdBindDescriptorSets(
        commandBuffer,
        VK_PIPELINE_BIND_POINT_GRAPHICS,
        _pipelineLayout,
        0, // first set number
        1, // setCount
        &_descriptorSets[frame.index],
        0, // dynamic offset count
        nullptr); // dynamic offsets
    vkCmdDrawIndexed(commandBuffer, _meshCore.indexCount(), 1, 0, 0, 0);
    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

vku::Framebuffer vku::ChunkRender::createFramebuffer(vku::SwapchainFrame &frame)
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

void vku::ChunkRender::onResize(size_t imageCount, VkExtent2D)
{
    // recreate the uniform buffers as the number of swapchain images could have changed
    _uboBufferMemories.clear();
    _uboBuffers.clear();

    _uboBuffers.resize(imageCount);
    _uboBufferMemories.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        _uboBuffers[i] = vku::Buffer::exclusive(_displayCore.device(), sizeof(vku::SimpleView), VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
        _uboBufferMemories[i] = vku::DeviceMemory::hostCoherentBuffer(
            _displayCore.physicalDevice(),
            _displayCore.device(),
            _uboBuffers[i]);
    }

    VkDescriptorPoolSize poolSizes[] = {
        vku::descriptorPoolSize(VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER, imageCount)
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
    }
}

void vku::ChunkRender::onUpdate(vku::SwapchainFrame &frame)
{
    static auto start = std::chrono::high_resolution_clock::now();

    auto now = std::chrono::high_resolution_clock::now();
    float time = std::chrono::duration<float, std::chrono::seconds::period>(now - start).count();

    float scale = 1.0f / _chunk.size;
    float offset = static_cast<float>(_chunk.size) / -2.0f;

    vku::SimpleView ubo = {};
    ubo.model = glm::identity<glm::mat4>();
    ubo.model = glm::rotate(ubo.model, time * glm::radians(90.f), glm::vec3(0.0f, 1.0f, 0.0f));
    ubo.model = glm::scale(ubo.model, glm::vec3(scale));
    ubo.model = glm::translate(ubo.model, glm::vec3(offset));
    ubo.view = glm::lookAt(glm::vec3(2.0f, 2.0f, 2.0f), glm::vec3(0.0f, 0.0f, 0.0f), glm::vec3(0.0f, 1.0f, 0.0f));

    auto extent = _swapchainCore.extent();
    ubo.projection = glm::perspective(
        glm::radians(45.0f),
        static_cast<float>(extent.width) / static_cast<float>(extent.height),
        0.1f,
        100.0f);
    ubo.projection[1][1] *= -1;
    vku::hostDeviceCopy(_displayCore.device(), &ubo, _uboBufferMemories[frame.index], sizeof(vku::SimpleView));
}
