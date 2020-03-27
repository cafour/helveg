#include "app.hpp"

#include <algorithm>
#include <vector>

vku::App::App(
    const std::string &name,
    int width,
    int height,
    const std::vector<const char *> *instanceExtensions,
    const std::vector<const char *> *layers,
    const std::vector<const char *> *deviceExtensions)
{
    _window = vku::Window::noApi(width, height, name);
    _instance = vku::Instance::basic(name, true, IS_DEBUG, instanceExtensions, layers, &_debugMessenger);
    _surface = vku::Surface::glfw(_instance, _window);

    size_t extensionCount = 1;
    if (deviceExtensions) {
        extensionCount += deviceExtensions->size();
    }
    std::vector<const char *> actualDeviceExtensions(extensionCount);
    actualDeviceExtensions[0] = VK_KHR_SWAPCHAIN_EXTENSION_NAME;
    if (deviceExtensions) {
        std::copy(deviceExtensions->begin(), deviceExtensions->end(), std::next(actualDeviceExtensions.begin()));
    }

    _physicalDevice = vku::findDevice(_instance, _surface, &_queueIndex, &actualDeviceExtensions);
    _device = vku::Device::basic(_physicalDevice, _queueIndex, &actualDeviceExtensions);
    auto surfaceFormat = vku::findSurfaceFormat(_physicalDevice, _surface);
    _renderPass = vku::RenderPass::basic(_device, surfaceFormat.format);
    _swapchainEnv = std::make_optional<vku::SwapchainEnv>(_device, _physicalDevice, _surface, _renderPass);
    _commandPool = vku::CommandPool::basic(_device, _queueIndex);
    vkGetDeviceQueue(_device, _queueIndex, 0, &_queue);
}

void vku::App::prepare()
{
    _commandBuffers.clear();
    _commandBuffers.resize(_swapchainEnv->frames().size(), VK_NULL_HANDLE);

    VkCommandBufferAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocateInfo.commandPool = _commandPool;
    allocateInfo.commandBufferCount = static_cast<uint32_t>(_commandBuffers.size());
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    ENSURE(vkAllocateCommandBuffers(_device, &allocateInfo, _commandBuffers.data()));

    for (size_t i = 0; i < _commandBuffers.size(); ++i) {
        recordCommands(_commandBuffers[i], _swapchainEnv->frames()[i]);
    }
}

void vku::App::run()
{
    prepare();
    while (!glfwWindowShouldClose(_window)) {
        glfwPollEvents();
        step();
    }

    ENSURE(vkDeviceWaitIdle(_device));
}

void vku::App::step()
{
    // acquire a frame
    vku::SwapchainFrame *frame;
    VkResult result = _swapchainEnv->acquire(frame);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        _swapchainEnv = vku::SwapchainEnv(
            _device,
            _physicalDevice,
            _surface,
            _renderPass,
            _swapchainEnv->swapchain());
        vkFreeCommandBuffers(_device, _commandPool, _commandBuffers.size(), _commandBuffers.data());
        prepare();
        return;
    } else if (result != VK_SUCCESS) {
        ENSURE(vkQueueWaitIdle(_queue));
        return;
    }

    // submit rendering work
    VkPipelineStageFlags waitStage { VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT };

    VkSubmitInfo submitInfo = {};
    submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
    submitInfo.commandBufferCount = 1;
    submitInfo.pCommandBuffers = &_commandBuffers[frame->index];
    submitInfo.waitSemaphoreCount = 1;
    submitInfo.pWaitSemaphores = frame->acquireSemaphore;
    submitInfo.pWaitDstStageMask = &waitStage;
    submitInfo.signalSemaphoreCount = 1;
    submitInfo.pSignalSemaphores = frame->releaseSemaphore;
    ENSURE(vkQueueSubmit(_queue, 1, &submitInfo, frame->fence));

    VkPresentInfoKHR presentInfo {};
    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.swapchainCount = 1;
    presentInfo.pSwapchains = _swapchainEnv->swapchain();
    presentInfo.pImageIndices = &frame->index;
    presentInfo.waitSemaphoreCount = 1;
    presentInfo.pWaitSemaphores = frame->releaseSemaphore;
    LOG(vkQueuePresentKHR(_queue, &presentInfo));
}
