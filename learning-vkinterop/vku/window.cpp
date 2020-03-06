#include "window.hpp"

#include <stdexcept>

void vku::Window::resizeCallback(GLFWwindow *glfwWindow, int width, int height)
{
    auto window = reinterpret_cast<vku::Window *>(glfwGetWindowUserPointer(glfwWindow));
    window->_onResize(window->_userData, width, height);
}

vku::Window::Window(
    int width,
    int height,
    const char *title,
    void (*onResize)(void *userData, int width, int height),
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
