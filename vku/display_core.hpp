#pragma once

#include "device_related.hpp"
#include "instance_related.hpp"
#include "standalone.hpp"
#include "window.hpp"

namespace vku {
/**
 * Handles device and surface creation for display purposes.
 */
class DisplayCore {
private:
    VkInstance _instance;
    vku::Window _window;
    VkPhysicalDevice _physicalDevice;
    vku::Device _device;
    vku::Surface _surface;
    VkSurfaceFormatKHR _surfaceFormat;
    uint32_t _queueIndex;
    VkQueue _queue;

    static const std::vector<const char *> defaultExtensions;

public:
    DisplayCore()
        : DisplayCore(VK_NULL_HANDLE, 1280, 720, "default")
    { }

    DisplayCore(
        VkInstance instance,
        int width,
        int height,
        const std::string &name,
        const VkPhysicalDeviceFeatures *features = nullptr)
        : DisplayCore(instance, width, height, name, defaultExtensions, features)
    { }

    DisplayCore(
        VkInstance instance,
        int width,
        int height,
        const std::string &name,
        const std::vector<const char *> &extensions,
        const VkPhysicalDeviceFeatures *features = nullptr,
        const void *deviceCreateInfoNext = nullptr);

    vku::Window &window() { return _window; }
    VkPhysicalDevice physicalDevice() { return _physicalDevice; }
    vku::Device &device() { return _device; }
    vku::Surface &surface() { return _surface; }
    VkSurfaceFormatKHR surfaceFormat() { return _surfaceFormat; }
    uint32_t queueIndex() { return _queueIndex; }
    VkQueue queue() { return _queue; }
};
}
