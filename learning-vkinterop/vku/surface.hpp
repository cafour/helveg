#pragma once

#include <volk.h>
#include <GLFW/glfw3.h>

namespace vku {

class Surface {
private:
    VkInstance _instance;
    VkSurfaceKHR _raw;

public:
    Surface(VkInstance instance, VkSurfaceKHR raw);
    ~Surface();
    Surface(const Surface& other) = delete;
    Surface(Surface &&other) noexcept;
    Surface &operator=(const Surface &other) = delete;
    Surface &operator=(Surface &&other) noexcept;

    operator VkSurfaceKHR() { return _raw; }
    VkSurfaceKHR raw() { return _raw; }
    VkInstance instance() { return _instance; }

    static Surface glfw(VkInstance instance, GLFWwindow *window);
};
}
