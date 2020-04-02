#pragma once

#include "device_related.hpp"

#include <volk.h>

#include <functional>
#include <vector>

namespace vku {

struct SwapchainFrame {
    uint32_t index;
    VkImage image;
    ImageView imageView;
    Semaphore acquireSemaphore;
    Semaphore releaseSemaphore;
    Fence fence;
};

class SwapchainEnv {
private:
    Swapchain _swapchain;
    std::vector<SwapchainFrame> _frames;
    std::vector<Semaphore> _recycledSemaphores;
    VkExtent2D _extent;

public:
    SwapchainEnv() {}
    SwapchainEnv(
        VkDevice device,
        VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface,
        VkSwapchainKHR old = VK_NULL_HANDLE);

    Swapchain &swapchain() { return _swapchain; }
    std::vector<SwapchainFrame> &frames() { return _frames; }
    VkExtent2D extent() const { return _extent; }

    VkResult acquire(SwapchainFrame *&frame);
};
}
