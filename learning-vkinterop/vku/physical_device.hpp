#pragma once

#include <volk.h>

namespace vku {
class PhysicalDevice {
private:
    VkPhysicalDevice _raw;

public:
    PhysicalDevice(VkPhysicalDevice raw)
        : _raw(raw)
    {
    }

    operator VkPhysicalDevice() { return _raw; }

    bool hasExtensions(VkPhysicalDevice device, const char **extensions, size_t length);
};
}
