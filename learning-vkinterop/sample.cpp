#include "sample.hpp"
#include "shaders.hpp"


void Sample::recordCommands()
{
    _commandBuffers.clear();
    _commandBuffers.resize(_swapchain.imageCount(), VK_NULL_HANDLE);

    VkCommandBufferAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocateInfo.commandPool = _commandPool;
    allocateInfo.commandBufferCount = static_cast<uint32_t>(_commandBuffers.size());
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

    ENSURE(vkAllocateCommandBuffers(_device, &allocateInfo, _commandBuffers.data()));

    for (size_t i = 0; i < _commandBuffers.size(); ++i) {
        VkCommandBufferBeginInfo beginInfo = {};
        beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;

        ENSURE(vkBeginCommandBuffer(_commandBuffers[i], &beginInfo));

        VkRenderPassBeginInfo renderPassInfo = {};
        renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO;
        renderPassInfo.renderPass = _renderPass;
        renderPassInfo.framebuffer = _swapchain.framebuffers()[i];
        renderPassInfo.renderArea.offset = { 0, 0 };
        renderPassInfo.renderArea.extent = _swapchain.extent();

        VkClearValue clearColor = { 0.0f, 0.0f, 0.0f, 1.0f };
        renderPassInfo.clearValueCount = 1;
        renderPassInfo.pClearValues = &clearColor;

        vkCmdBeginRenderPass(_commandBuffers[i], &renderPassInfo, VK_SUBPASS_CONTENTS_INLINE);
        vkCmdBindPipeline(_commandBuffers[i], VK_PIPELINE_BIND_POINT_GRAPHICS, _pipeline);
        vkCmdDraw(_commandBuffers[i], 3, 1, 0, 0);
        vkCmdEndRenderPass(_commandBuffers[i]);

        ENSURE(vkEndCommandBuffer(_commandBuffers[i]));
    }
}

void Sample::step()
{
    // acquire a frame
    vku::SwapchainFrame frame;
    VkResult result = _swapchain.acquire(frame);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        vkDeviceWaitIdle(_device);
        _swapchain.rebuild();
        vkFreeCommandBuffers(_device, _commandPool, _commandBuffers.size(), _commandBuffers.data());
        recordCommands();
    } else if (result != VK_SUCCESS) {
        ENSURE(vkQueueWaitIdle(_queue));
        return;
    }

    // submit rendering work
    VkPipelineStageFlags waitStage { VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT };

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &_commandBuffers[frame.index];
    submitInfo.waitSemaphoreCount = 1;
    submitInfo.pWaitSemaphores = &frame.acquireSemaphore;
    submitInfo.pWaitDstStageMask = &waitStage;
    submitInfo.signalSemaphoreCount = 1;
    submitInfo.pSignalSemaphores = &frame.releaseSemaphore;
    ENSURE(vkQueueSubmit(_queue, 1, &submitInfo, frame.fence));

    VkPresentInfoKHR presentInfo {};
    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.swapchainCount = 1;
    presentInfo.pSwapchains = _swapchain.raw();
    presentInfo.pImageIndices = &frame.index;
    presentInfo.waitSemaphoreCount = 1;
    presentInfo.pWaitSemaphores = &frame.releaseSemaphore;
    LOG(vkQueuePresentKHR(_queue, &presentInfo));
}

void Sample::run()
{
    while (!glfwWindowShouldClose(_window)) {
        glfwPollEvents();
        step();
    }

    vkDeviceWaitIdle(_device);
}
