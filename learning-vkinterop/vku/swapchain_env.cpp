#include "swapchain_env.hpp"
#include "base.hpp"
#include "physical_device.hpp"

#include <vector>

vku::SwapchainEnv::SwapchainEnv(
    VkDevice device,
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface,
    VkRenderPass renderPass,
    VkSwapchainKHR old)
{
    VkSurfaceCapabilitiesKHR capabilities;
    VkSurfaceFormatKHR format;
    auto swapchain = vku::Swapchain::basic(device, physicalDevice, surface, &capabilities, &format, old);
    uint32_t imageCount = 0;
    ENSURE(vkGetSwapchainImagesKHR(device, _swapchain, &imageCount, nullptr));
    std::vector<VkImage> images(imageCount, VK_NULL_HANDLE);
    ENSURE(vkGetSwapchainImagesKHR(device, swapchain, &imageCount, images.data()));

    for (size_t i = 0; i < imageCount; ++i) {
        auto imageView = vku::ImageView::basic(device, images[i], format.format);
        auto framebuffer = vku::Framebuffer::basic(
            device,
            renderPass,
            imageView,
            capabilities.currentExtent.width,
            capabilities.currentExtent.height);
        _frames.push_back(vku::SwapchainFrame {
            static_cast<uint32_t>(i),
            images[i],
            std::move(imageView),
            std::move(framebuffer),
            vku::Semaphore::basic(device),
            vku::Semaphore::basic(device),
            vku::Fence::basic(device) });
    }
}

VkResult vku::SwapchainEnv::acquire(vku::SwapchainFrame *&frame)
{
    vku::Semaphore acquireSemaphore = _recycledSemaphores.empty()
        ? vku::Semaphore::basic(_swapchain.device())
        : std::move(_recycledSemaphores.back());
    if (!_recycledSemaphores.empty()) {
        _recycledSemaphores.pop_back();
    }

    uint32_t index;
    VkResult result = vkAcquireNextImageKHR(
        _swapchain.device(),
        _swapchain,
        UINT64_MAX,
        acquireSemaphore, // the semaphore is signaled when the swapchain is done reading
        VK_NULL_HANDLE,
        &index);
    if (result != VK_SUCCESS) {
        _recycledSemaphores.emplace_back(std::move(acquireSemaphore));
        return result;
    }

    frame = &_frames[index];
    frame->index = index;

    // wait for the all command buffers to finish executing
    VkFence indexFence = frame->fence;
    ENSURE(vkWaitForFences(_swapchain.device(), 1, &indexFence, true, UINT64_MAX));
    ENSURE(vkResetFences(_swapchain.device(), 1, &indexFence));

    _recycledSemaphores.emplace_back(std::move(frame->acquireSemaphore));
    frame->acquireSemaphore = std::move(acquireSemaphore);

    return VK_SUCCESS;
}
