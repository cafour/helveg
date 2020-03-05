#pragma once

#include <volk.h>

#include <string>
#include <vector>

struct QueueIndices {
    uint32_t graphics = -1;
    uint32_t present = -1;

    bool isComplete() { return graphics != -1 && present != -1; }
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


