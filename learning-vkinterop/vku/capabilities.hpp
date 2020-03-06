#pragma once

#include <volk.h>

#include <vector>

namespace vku {
struct QueueIndices {
    uint32_t graphics = -1;
    uint32_t present = -1;

    bool isComplete() { return graphics != -1u && present != -1u; }
};

struct SwapchainDetails {
    VkSurfaceCapabilitiesKHR capabilities;
    std::vector<VkSurfaceFormatKHR> formats;
    std::vector<VkPresentModeKHR> presentModes;

    VkSurfaceFormatKHR pickFormat();
    VkPresentModeKHR pickPresentMode();
    VkExtent2D pickExtent(uint32_t width, uint32_t height);
};

QueueIndices getQueueIndices(VkPhysicalDevice device, VkSurfaceKHR surface);
SwapchainDetails getSwapchainDetails(VkPhysicalDevice device, VkSurfaceKHR surface);
}
