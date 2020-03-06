#pragma once

#include "device.hpp"

#include <vector>
#include <volk.h>

namespace vku {

class Swapchain {
private:
    VkSwapchainKHR _raw;
    Device &_device;
    std::vector<VkImage> _swapchainImages;
    std::vector<VkImageView> _swapchainImageViews;

public:
    Swapchain(Device &device);
    ~Swapchain();

    operator VkSwapchainKHR() { return _raw; }

    Device &device() { return _device; }
};
}
