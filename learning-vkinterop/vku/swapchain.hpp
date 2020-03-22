#pragma once

#include <volk.h>

namespace vku {

class Swapchain {
private:
    VkDevice _device;
    VkSwapchainKHR _raw;

public:
    Swapchain(VkDevice device, VkSwapchainKHR raw);
    Swapchain(VkDevice device, VkSwapchainCreateInfoKHR &createInfo);
    ~Swapchain();
    Swapchain(const Swapchain &other) = delete;
    Swapchain(Swapchain &&other) noexcept;
    Swapchain &operator=(const Swapchain &other) = delete;
    Swapchain &operator=(Swapchain &&other) noexcept;

    operator VkSwapchainKHR() { return _raw; }
    VkSwapchainKHR raw() { return _raw; }
    VkDevice device() { return _device; }

    static Swapchain basic(
        VkDevice device,
        VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface,
        VkSwapchainKHR old = VK_NULL_HANDLE);
};
}
