#include "triangle_render.hpp"
#include "shaders.hpp"

vku::TriangleRender::TriangleRender(int width, int height)
    : _instanceCore("TriangleRender", true, true)
    , _displayCore(_instanceCore.instance(), width, height, "vkdev")
    , _swapchainCore(_displayCore)
    , _renderCore(
          _displayCore,
          _swapchainCore,
          [this](auto &f) { return this->createFramebuffer(f); },
          [this](auto cb, auto &f) { this->recordCommandBuffer(cb, f); })
    , _renderPass(vku::RenderPass::basic(_displayCore.device(), _displayCore.surfaceFormat().format))
    , _pipelineLayout(vku::PipelineLayout::basic(_displayCore.device()))
{
    _pipeline = vku::GraphicsPipeline::basic(
        _displayCore.device(),
        _pipelineLayout,
        _renderPass,
        vku::ShaderModule::inlined(_displayCore.device(), TRIANGLE_VERT, TRIANGLE_VERT_LENGTH),
        vku::ShaderModule::inlined(_displayCore.device(), TRIANGLE_FRAG, TRIANGLE_FRAG_LENGTH));
}

void vku::TriangleRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
    VkCommandBufferBeginInfo beginInfo = {};
    beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
    ENSURE(vkBeginCommandBuffer(commandBuffer, &beginInfo));

    VkRenderPassBeginInfo renderPassInfo = {};
    renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
    renderPassInfo.renderPass = _renderPass;
    renderPassInfo.framebuffer = _renderCore.framebuffers()[frame.index];

    VkExtent2D extent = _swapchainCore.extent();
    renderPassInfo.renderArea.extent = extent;

    VkClearValue clearColor = { 0.0f, 0.0f, 0.0f, 1.0f };
    renderPassInfo.clearValueCount = 1;
    renderPassInfo.pClearValues = &clearColor;

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
    VkDeviceSize offset = 0;
    vkCmdDraw(commandBuffer, 3, 1, 0, 0);
    vkCmdEndRenderPass(commandBuffer);

    ENSURE(vkEndCommandBuffer(commandBuffer));
}

vku::Framebuffer vku::TriangleRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    auto extent = _swapchainCore.extent();
    return vku::Framebuffer::basic(_displayCore.device(), _renderPass, frame.imageView, 1, extent.width, extent.height);
}
