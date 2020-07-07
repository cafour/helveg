#pragma once

#include "types.hpp"

#include <volk.h>

#include <GLFW/glfw3.h>

namespace vku {

class DebugMessenger : public InstanceConstructible<
                           VkDebugUtilsMessengerEXT,
                           VkDebugUtilsMessengerCreateInfoEXT,
                           &vkCreateDebugUtilsMessengerEXT,
                           &vkDestroyDebugUtilsMessengerEXT> {
public:
    using InstanceConstructible::InstanceConstructible;

    static VkDebugUtilsMessengerCreateInfoEXT vkuLogCreateInfo();
    static DebugMessenger vkuLog(VkInstance instance);
};

class Surface : public InstanceRelated<VkSurfaceKHR> {
public:
    using InstanceRelated::InstanceRelated;
    ~Surface()
    {
        if (_instance != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            vkDestroySurfaceKHR(_instance, _raw, nullptr);
        }
    }
    Surface(Surface &&other) noexcept = default;
    Surface &operator=(Surface &&other) noexcept = default;

    static Surface glfw(VkInstance instance, GLFWwindow *window);
};

}
