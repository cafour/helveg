#pragma once

#include <volk.h>

namespace vku {

class DebugMessenger {
private:
    VkInstance _instance;
    VkDebugUtilsMessengerEXT _raw;

public:
    DebugMessenger(VkInstance instance, VkDebugUtilsMessengerEXT raw);
    DebugMessenger(VkInstance instance, VkDebugUtilsMessengerCreateInfoEXT &createInfo);
    ~DebugMessenger();
    DebugMessenger(const DebugMessenger &other) = delete;
    DebugMessenger(DebugMessenger &&other) noexcept;
    DebugMessenger &operator=(const DebugMessenger &other) = delete;
    DebugMessenger &operator=(DebugMessenger &&other) noexcept;

    operator VkDebugUtilsMessengerEXT() { return _raw; }
    VkDebugUtilsMessengerEXT raw() { return _raw; }
    VkInstance instance() { return _instance; }

    static VkDebugUtilsMessengerCreateInfoEXT cerrCreateInfo();
    static DebugMessenger cerr(VkInstance instance);
};
}
