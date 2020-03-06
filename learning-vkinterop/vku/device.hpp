#pragma once

#include "physical_device.hpp"

#include <volk.h>

#include <string>
#include <vector>

namespace vku {

class Device {
private:
    VkDevice _raw;
    PhysicalDevice &_physicalDevice;
    std::vector<std::string> _extensions;
    std::vector<const char *> _extensionPtrs;

public:
    Device(
        PhysicalDevice &physicalDevice,
        const char *const *extensions = nullptr,
        size_t extensionCount = 0);
    ~Device() { vkDestroyDevice(_raw, nullptr); }

    operator VkDevice() { return _raw; }

    PhysicalDevice &physicalDevice() { return _physicalDevice; }
};
}
