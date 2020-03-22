#pragma once

#include "physical_device.hpp"

#include <volk.h>

#include <string>
#include <vector>

namespace vku {

class Device {
private:
    VkDevice _raw;

public:
    Device(VkDevice raw);
    Device(VkPhysicalDevice physicalDevice, VkDeviceCreateInfo &createInfo);
    ~Device();
    Device(const Device &other) = delete;
    Device(Device &&other) noexcept;
    Device &operator=(const Device &other) = delete;
    Device &operator=(Device &&other) noexcept;

    operator VkDevice() { return _raw; }
    VkDevice raw() { return _raw; }

    static Device basic(
        VkPhysicalDevice physicalDevice,
        uint32_t queueIndex,
        const std::vector<const char*> *extensions = nullptr);
};
}
