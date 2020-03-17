#pragma once

#include "capabilities.hpp"
#include "instance.hpp"
#include "surface.hpp"

#include <volk.h>

namespace vku {

class PhysicalDevice {
private:
    VkPhysicalDevice _raw;
    Instance &_instance;
    Surface &_surface;
    uint32_t _queueIndex = -1;

public:
    PhysicalDevice(
        Instance &instance,
        Surface &surface,
        const char **extensions = nullptr,
        size_t length = 0);

    operator VkPhysicalDevice() { return _raw; }

    Instance &instance() { return _instance; }
    Surface &surface() { return _surface; }
    uint32_t queueIndex() { return _queueIndex; }
    VkSurfaceFormatKHR surfaceFormat();
    VkSurfaceCapabilitiesKHR surfaceCapabilities();
};
}
