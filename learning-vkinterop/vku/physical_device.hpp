#pragma once

#include "capabilities.hpp"
#include "instance.hpp"
#include "surface.hpp"

#include <vector>
#include <volk.h>

namespace vku {

bool hasExtensionSupport(
    VkPhysicalDevice physicalDevice,
    const std::vector<const char *> &extensions);

VkPhysicalDevice findDevice(
    VkInstance instance,
    VkSurfaceKHR surface,
    uint32_t *queueIndex,
    const std::vector<const char *> *requiredExtensions = nullptr);

VkSurfaceFormatKHR findSurfaceFormat(
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface);
}
