#include "window.hpp"

#include <stdexcept>

size_t vku::Window::count = 0;

vku::Window::Window(
    int width,
    int height,
    const std::string &title)
    : _width(width)
    , _height(height)
{
    if (count == 0 && glfwInit() == GLFW_FALSE) {
        throw std::runtime_error("GLFW failed to initialize");
    }
    count++;
    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    _raw = glfwCreateWindow(width, height, title.c_str(), nullptr, nullptr);
}

vku::Window::~Window()
{
    glfwDestroyWindow(_raw);
    count--;
    if (count == 0) {
        glfwTerminate();
    }
}
