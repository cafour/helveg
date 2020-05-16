#pragma once

#include "types.hpp"

#include <volk.h>

#include <optional>
#include <string>
#include <vector>

namespace vku {
class DebugMessenger;

class Instance : public StandaloneConstructible<
                     VkInstance,
                     VkInstanceCreateInfo,
                     &vkCreateInstance,
                     &vkDestroyInstance> {
public:
    using StandaloneConstructible::StandaloneConstructible;
    Instance(VkInstanceCreateInfo &createInfo)
        : StandaloneConstructible(createInfo)
    {
        volkLoadInstance(_raw);
    }
    Instance(Instance &&other) noexcept = default;
    Instance &operator=(Instance &&other) noexcept = default;

    static Instance basic(
        const std::string &appName,
        bool usesGlfw = true,
        bool isDebug = false,
        const std::vector<const char *> *extensions = nullptr,
        const std::vector<const char *> *layers = nullptr,
        std::optional<vku::DebugMessenger> *messenger = nullptr);
};

class Device : public Standalone<VkDevice>{
protected:
    VkPhysicalDevice _physicalDevice = VK_NULL_HANDLE;

public:
    using Standalone::Standalone;
    Device(VkPhysicalDevice physicalDevice, VkDeviceCreateInfo &createInfo)
        : _physicalDevice(physicalDevice)
    {
        ENSURE(vkCreateDevice(physicalDevice, &createInfo, nullptr, &_raw));
    }
    ~Device()
    {
        if (_raw != VK_NULL_HANDLE) {
            vkDestroyDevice(_raw, nullptr);
        }
    }
    Device(Device &&other) noexcept = default;
    Device &operator=(Device &&other) noexcept = default;

    VkPhysicalDevice physicalDevice() { return _physicalDevice; }

    static Device basic(
        VkPhysicalDevice physicalDevice,
        uint32_t queueIndex,
        const std::vector<const char*> *extensions = nullptr,
        const VkPhysicalDeviceFeatures *features = nullptr);
};
}
