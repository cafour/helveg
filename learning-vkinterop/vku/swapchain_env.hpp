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
    Framebuffer framebuffer;
    Semaphore acquireSemaphore;
    Semaphore releaseSemaphore;
    Fence fence;
};

class SwapchainEnv {
private:
    Swapchain _swapchain;
    std::vector<SwapchainFrame> _frames;
    std::vector<Semaphore> _recycledSemaphores;

public:
    SwapchainEnv(Swapchain &&swapchain);

    Swapchain &swapchain() { return _swapchain; }
    VkResult acquire(SwapchainFrame **frame);

    static SwapchainEnv basic(
        VkDevice device,
        VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface,
        VkRenderPass renderPass,
        VkSwapchainKHR old = VK_NULL_HANDLE);
};
}
