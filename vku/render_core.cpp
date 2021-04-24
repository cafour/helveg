#include "render_core.hpp"

vku::RenderCore::RenderCore(
    vku::DisplayCore &displayCore,
    vku::SwapchainCore &swapchainCore,
    std::function<vku::Framebuffer(vku::SwapchainFrame &)> createFramebuffer,
    std::function<void(VkCommandBuffer, vku::SwapchainFrame &)> recordCommandBuffer)
    : _displayCore(displayCore)
    , _swapchainCore(swapchainCore)
    , _createFramebuffer(createFramebuffer)
    , _recordCommandBuffer(recordCommandBuffer)
{
    _commandPool = vku::CommandPool::basic(displayCore.device(), displayCore.queueIndex());
}

void vku::RenderCore::run()
{
    resize();
    while (!glfwWindowShouldClose(_displayCore.window())) {
        glfwPollEvents();
        step();
    }

    ENSURE(vkDeviceWaitIdle(_displayCore.device()));
}

void vku::RenderCore::step()
{
    vku::SwapchainFrame *frame;
    VkResult result = _swapchainCore.acquire(frame);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        resize();
        return;
    } else if (result != VK_SUCCESS) {
        ENSURE(vkQueueWaitIdle(_displayCore.queue()));
        return;
    }

    for (auto handler : _updateHandlers) {
        handler(*frame);
    }

    VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &_commandBuffers[frame->index];
    submitInfo.waitSemaphoreCount = 1;
    submitInfo.pWaitSemaphores = frame->acquireSemaphore;
    submitInfo.pWaitDstStageMask = &waitStage;
    submitInfo.signalSemaphoreCount = 1;
    submitInfo.pSignalSemaphores = frame->releaseSemaphore;
    ENSURE(vkQueueSubmit(_displayCore.queue(), 1, &submitInfo, frame->fence));

    VkPresentInfoKHR presentInfo = {};
    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.swapchainCount = 1;
    presentInfo.pSwapchains = _swapchainCore.swapchain();
    presentInfo.pImageIndices = &frame->index;
    presentInfo.waitSemaphoreCount = 1;
    presentInfo.pWaitSemaphores = frame->releaseSemaphore;
    result = vkQueuePresentKHR(_displayCore.queue(), &presentInfo);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        resize();
        return;
    } else {
        ENSURE(result);
    }
}

void vku::RenderCore::resize()
{
    _swapchainCore = vku::SwapchainCore(
        _displayCore,
        _swapchainCore.swapchain());

    for (auto handler : _resizeHandlers) {
        handler(_swapchainCore.frames().size(), _swapchainCore.extent());
    }

    _framebuffers.resize(_swapchainCore.frames().size());
    for (auto &frame : _swapchainCore.frames()) {
        _framebuffers[frame.index] = _createFramebuffer(frame);
    }

    VkCommandBufferAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocateInfo.commandPool = _commandPool;
    allocateInfo.commandBufferCount = static_cast<uint32_t>(_swapchainCore.frames().size());
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    _commandBuffers = vku::CommandBuffers(_displayCore.device(), allocateInfo);

    for (size_t i = 0; i < _commandBuffers.size(); ++i) {
        _recordCommandBuffer(_commandBuffers[i], _swapchainCore.frames()[i]);
    }
}

void vku::RenderCore::onUpdate(std::function<void(vku::SwapchainFrame &)> handler)
{
    _updateHandlers.push_back(handler);
}

void vku::RenderCore::onResize(std::function<void(size_t, VkExtent2D)> handler)
{
    _resizeHandlers.push_back(handler);
}
