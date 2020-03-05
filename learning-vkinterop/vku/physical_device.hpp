#pragma once

#include <volk.h>

namespace vku {
class PhysicalDevice {
private:
    VkPhysicalDevice _raw;

public:
    operator VkPhysicalDevice() { return _raw; }

    bool hasExtensions(VkPhysicalDevice device, const char **extensions, size_t length);
};
}
