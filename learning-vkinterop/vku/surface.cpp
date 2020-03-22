#include "surface.hpp"
#include "base.hpp"

#include <utility>

vku::Surface::Surface(VkInstance instance, VkSurfaceKHR raw)
    : _instance(instance)
    , _raw(raw)
{
}

vku::Surface::~Surface()
{
    if (_instance != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
        vkDestroySurfaceKHR(_instance, _raw, nullptr);
    }
}

vku::Surface::Surface(Surface &&other)
    : _instance(std::exchange(other._instance, VK_NULL_HANDLE))
    , _raw(std::exchange(other._raw, VK_NULL_HANDLE))
{
}

vku::Surface& vku::Surface::operator=(vku::Surface &&other)
{
    if (this != &other) {
        _instance = std::exchange(other._instance, VK_NULL_HANDLE);
        _raw = std::exchange(other._raw, VK_NULL_HANDLE);
    }
    return *this;
}

vku::Surface vku::Surface::glfw(VkInstance instance, GLFWwindow *window)
{
    VkSurfaceKHR raw;
    ENSURE(glfwCreateWindowSurface(instance, window, nullptr, &raw));
    return vku::Surface(instance, raw);
}
