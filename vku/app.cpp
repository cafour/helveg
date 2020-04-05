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
    vkGetDeviceQueue(_device, _queueIndex, 0, &_queue);
    _commandPool = vku::CommandPool::basic(_device, _queueIndex);

    auto surfaceFormat = vku::findSurfaceFormat(physicalDevice(), surface());
    const std::vector<VkFormat> depthFormats {
        VK_FORMAT_D32_SFLOAT_S8_UINT,
        VK_FORMAT_D24_UNORM_S8_UINT
    };
    _depthFormat = vku::findSupportedFormat(
        physicalDevice(),
        depthFormats,
        VK_IMAGE_TILING_OPTIMAL,
        VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT);
    _renderPass = vku::RenderPass::basic(_device, surfaceFormat.format, _depthFormat);
}

void vku::App::run()
{
    resize();
    while (!glfwWindowShouldClose(_window)) {
        glfwPollEvents();
        step();
    }

    ENSURE(vkDeviceWaitIdle(_device));
}

void vku::App::step()
{
    vku::SwapchainFrame *frame;
    VkResult result = _swapchainEnv.acquire(frame);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        resize();
        return;
    } else if (result != VK_SUCCESS) {
        ENSURE(vkQueueWaitIdle(_queue));
        return;
    }

    update(*frame);

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
    ENSURE(vkQueueSubmit(_queue, 1, &submitInfo, frame->fence));

    VkPresentInfoKHR presentInfo {};
    presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
    presentInfo.swapchainCount = 1;
    presentInfo.pSwapchains = _swapchainEnv.swapchain();
    presentInfo.pImageIndices = &frame->index;
    presentInfo.waitSemaphoreCount = 1;
    presentInfo.pWaitSemaphores = frame->releaseSemaphore;
    result = vkQueuePresentKHR(_queue, &presentInfo);
    if (result == VK_SUBOPTIMAL_KHR || result == VK_ERROR_OUT_OF_DATE_KHR) {
        resize();
        return;
    } else {
        ENSURE(result);
    }
}

void vku::App::resize()
{
    _swapchainEnv = vku::SwapchainEnv(
        _device,
        _physicalDevice,
        _surface,
        _swapchainEnv.swapchain());
    VkExtent3D depthExtent {
        _swapchainEnv.extent().width,
        _swapchainEnv.extent().height,
        1
    };
    _depthImage = vku::Image::basic(
        _device,
        depthExtent,
        _depthFormat,
        VK_IMAGE_TILING_OPTIMAL,
        VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT);
    _depthImageMemory = vku::DeviceMemory::forImage(
        _physicalDevice,
        _device,
        _depthImage,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    _depthImageView = vku::ImageView::basic(
        _device,
        _depthImage,
        _depthFormat,
        VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT);

    _framebuffers.resize(_swapchainEnv.frames().size());
    for (auto &frame : _swapchainEnv.frames()) {
        VkImageView attachments[] = {
            frame.imageView,
            _depthImageView
        };
        _framebuffers[frame.index] = vku::Framebuffer::basic(
            _device,
            _renderPass,
            attachments,
            2,
            depthExtent.width,
            depthExtent.height);
    }

    VkCommandBufferAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
    allocateInfo.commandPool = _commandPool;
    allocateInfo.commandBufferCount = static_cast<uint32_t>(_swapchainEnv.frames().size());
    allocateInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
    _commandBuffers = vku::CommandBuffers(_device, allocateInfo);
    prepare();
    for (size_t i = 0; i < _commandBuffers.size(); ++i) {
        recordCommands(_commandBuffers[i], _swapchainEnv.frames()[i]);
    }
}
