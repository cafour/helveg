#include "window.hpp"

#include <stdexcept>
#include <utility>

size_t vku::Window::count = 0;

void vku::Window::handleMouseMove(GLFWwindow *raw, double x, double y)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_mouseMoveHandlers) {
        handler(x, y);
    }
}

void vku::Window::handleMousePress(GLFWwindow *raw, int button, int action, int mods)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_mousePressHandlers) {
        handler(button, action, mods);
    }
}

void vku::Window::handleKeyPress(GLFWwindow *raw, int key, int scancode, int action, int mods)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_keyPressHandlers) {
        handler(key, scancode, action, mods);
    }
}

vku::Window::Window(GLFWwindow *raw)
    : _raw(raw)
{
    count++;
    glfwSetWindowUserPointer(_raw, this);
    glfwSetCursorPosCallback(_raw, vku::Window::handleMouseMove);
    glfwSetMouseButtonCallback(_raw, vku::Window::handleMousePress);
    glfwSetKeyCallback(_raw, vku::Window::handleKeyPress);
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
    glfwSetWindowUserPointer(_raw, this);
}

vku::Window &vku::Window::operator=(vku::Window &&other) noexcept
{
    if (this != &other) {
        std::swap(_raw, other._raw);
        glfwSetWindowUserPointer(_raw, this);
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

void vku::Window::onMouseMove(std::function<void(double x, double y)> handler)
{
    _mouseMoveHandlers.push_back(handler);
}

void vku::Window::onMousePress(std::function<void(int button, int action, int mods)> handler)
{
    _mousePressHandlers.push_back(handler);
}

void vku::Window::onKeyPress(std::function<void(int key, int scancode, int action, int mods)> handler)
{
    _keyPressHandlers.push_back(handler);
}
