#include "swapchain_env.hpp"
#include "base.hpp"

#include <vector>

vku::SwapchainEnv::SwapchainEnv(
    VkDevice device,
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface,
    VkSwapchainKHR old)
{
    VkSurfaceCapabilitiesKHR capabilities;
    VkSurfaceFormatKHR format;
    _swapchain = vku::Swapchain::basic(device, physicalDevice, surface, &capabilities, &format, old);
    _extent = capabilities.currentExtent;
    uint32_t imageCount = 0;
    ENSURE(vkGetSwapchainImagesKHR(device, _swapchain, &imageCount, nullptr));
    std::vector<VkImage> images(imageCount, VK_NULL_HANDLE);
    ENSURE(vkGetSwapchainImagesKHR(device, _swapchain, &imageCount, images.data()));

    for (size_t i = 0; i < imageCount; ++i) {
        auto imageView = vku::ImageView::basic(device, images[i], format.format, VK_IMAGE_ASPECT_COLOR_BIT);
        _frames.push_back(vku::SwapchainFrame {
            static_cast<uint32_t>(i),
            images[i],
            std::move(imageView),
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
