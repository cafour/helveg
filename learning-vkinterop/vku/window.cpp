#include "window.hpp"

#include <stdexcept>

void vku::Window::resizeCallback(GLFWwindow *glfwWindow, int width, int height)
{
    auto window = reinterpret_cast<vku::Window *>(glfwGetWindowUserPointer(glfwWindow));
    if (!window) {
        throw std::runtime_error("pointer to a vku::Window has been lost");
    }

    window->_width = width;
    window->_height = height;
    window->_onResize(*window, window->_userData);
}

vku::Window::Window(
    int width,
    int height,
    const char *title,
    void (*onResize)(Window &window, void *userData),
    void *userData)
    : _width(width)
    , _height(height)
    , _onResize(onResize)
    , _userData(userData)
{
    if (_count == 0 && glfwInit() == GLFW_FALSE) {
        throw std::runtime_error("GLFW failed to initialize");
    }
    _count++;
    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    _raw = glfwCreateWindow(width, height, title, nullptr, nullptr);
    glfwSetFramebufferSizeCallback(_raw, resizeCallback);
}

vku::Window::~Window()
{
    glfwDestroyWindow(_raw);
    _count--;
    if (_count == 0) {
        glfwTerminate();
    }
}
