#include "window.hpp"

#include <stdexcept>
#include <utility>

size_t vku::Window::count = 0;

vku::Window::Window(GLFWwindow *raw)
    : _raw(raw)
{
    count++;
}

vku::Window::~Window()
{
    if (_raw) {
        glfwDestroyWindow(_raw);
        count--;
        if (count == 0) {
            glfwTerminate();
        }
    }
}

vku::Window::Window(vku::Window &&other) noexcept
    : _raw(std::exchange(other._raw, nullptr))
{
}

vku::Window &vku::Window::operator=(vku::Window &&other) noexcept
{
    if (this != &other) {
        std::swap(_raw, other._raw);
    }
    return *this;
}

vku::Window vku::Window::noApi(int width, int height, const std::string &title)
{
    if (count == 0 && glfwInit() == GLFW_FALSE) {
        throw std::runtime_error("GLFW failed to initialize");
    }
    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    auto raw = glfwCreateWindow(width, height, title.c_str(), nullptr, nullptr);
    return vku::Window(raw);
}
