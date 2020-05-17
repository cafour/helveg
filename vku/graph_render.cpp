#include "graph_render.hpp"
#include "shaders.hpp"

#include <chrono>
#include <glm/gtc/matrix_transform.hpp>

static vku::GraphicsPipeline buildPipeline(VkDevice device, VkPipelineLayout pipelineLayout, VkRenderPass renderPass)
{
    VkVertexInputBindingDescription vertexBindings[] = {
        vku::vertexInputBinding(0, sizeof(glm::vec2), VK_VERTEX_INPUT_RATE_VERTEX)
    };

    VkVertexInputAttributeDescription vertexAttributes[] = {
        vku::vertexInputAttribute(0, 0, VK_FORMAT_R32G32_SFLOAT, 0)
    };

    VkGraphicsPipelineCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;

    auto vertexInput = vku::vertexInputState(vertexBindings, 1, vertexAttributes, 1);
    createInfo.pVertexInputState = &vertexInput;

    auto inputAssembly = vku::inputAssemblyState(VK_PRIMITIVE_TOPOLOGY_POINT_LIST);
    createInfo.pInputAssemblyState = &inputAssembly;

    auto rasterization = vku::rasterizationState(
        VK_POLYGON_MODE_FILL,
        VK_CULL_MODE_BACK_BIT,
        VK_FRONT_FACE_COUNTER_CLOCKWISE);
    createInfo.pRasterizationState = &rasterization;

    auto multisample = vku::multisampleState(VK_SAMPLE_COUNT_1_BIT);
    createInfo.pMultisampleState = &multisample;

    auto colorBlendAttachment = vku::colorBlendAttachment(false);
    auto colorBlend = vku::colorBlendState(&colorBlendAttachment, 1);
    createInfo.pColorBlendState = &colorBlend;

    auto viewport = vku::viewportState(nullptr, 1, nullptr, 1);
    createInfo.pViewportState = &viewport;

    auto vertexShader = vku::ShaderModule::inlined(device, GRAPH_VERT, GRAPH_VERT_LENGTH);
    auto geometryShader = vku::ShaderModule::inlined(device, GRAPH_GEOM, GRAPH_GEOM_LENGTH);
    auto fragmentShader = vku::ShaderModule::inlined(device, GRAPH_FRAG, GRAPH_FRAG_LENGTH);

    VkPipelineShaderStageCreateInfo shaderStages[] = {
        vku::shaderStage(VK_SHADER_STAGE_VERTEX_BIT, vertexShader),
        vku::shaderStage(VK_SHADER_STAGE_GEOMETRY_BIT, geometryShader),
        vku::shaderStage(VK_SHADER_STAGE_FRAGMENT_BIT, fragmentShader)
    };
    createInfo.pStages = shaderStages;
    createInfo.stageCount = 3;

    VkDynamicState dynamicStates[] = { VK_DYNAMIC_STATE_VIEWPORT, VK_DYNAMIC_STATE_SCISSOR };
    auto dynamic = vku::dynamicState(dynamicStates, 2);
    createInfo.pDynamicState = &dynamic;

    createInfo.layout = pipelineLayout;
    createInfo.renderPass = renderPass;
    createInfo.subpass = 0;
    return vku::GraphicsPipeline(device, createInfo);
}

static VkPhysicalDeviceFeatures getDeviceFeatures()
{
    VkPhysicalDeviceFeatures features = {};
    features.geometryShader = VK_TRUE;
    return features;
}

static const VkPhysicalDeviceFeatures requiredFeatures = getDeviceFeatures();

GraphRender::GraphRender(int width, int height, Graph graph)
    : _instanceCore("GraphRender", true, true)
    , _displayCore(_instanceCore.instance(), width, height, "vkdev", &requiredFeatures)
    , _swapchainCore(_displayCore.device(), _displayCore.physicalDevice(), _displayCore.surface())
    , _renderCore(
          _displayCore,
          _swapchainCore,
          [this](auto &f) { return this->createFramebuffer(f); },
          [this](auto cb, auto &f) { this->recordCommandBuffer(cb, f); })
    , _pipelineLayout(vku::PipelineLayout::basic(_displayCore.device()))
    , _renderPass(vku::RenderPass::basic(_displayCore.device(), _displayCore.surfaceFormat().format))
    , _pipeline(buildPipeline(_displayCore.device(), _pipelineLayout, _renderPass))
    , _graph(graph)
{
    size_t positionsSize = graph.count * sizeof(glm::vec2);
    _positionBuffer = vku::Buffer::exclusive(
        _displayCore.device(),
        positionsSize,
        VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    _positionBufferMemory = vku::DeviceMemory::hostCoherentBuffer(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        _positionBuffer);
    flushPositions();
}

void GraphRender::flushPositions()
{
    vku::hostDeviceCopy(
        _displayCore.device(),
        _graph.positions,
        _positionBufferMemory,
        _graph.count * sizeof(glm::vec2),
        0);
}

void GraphRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
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

    VkDeviceSize offsets[] = { 0 };
    vkCmdBindVertexBuffers(commandBuffer, 0, 1, _positionBuffer, offsets);
    vkCmdDraw(commandBuffer, _graph.count, 1, 0, 0);
    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

vku::Framebuffer GraphRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    auto extent = _swapchainCore.extent();
    return vku::Framebuffer::basic(_displayCore.device(), _renderPass, frame.imageView, 1, extent.width, extent.height);
}
