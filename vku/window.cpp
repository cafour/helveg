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

void vku::Window::handleMouseButton(GLFWwindow *raw, int button, int action, int mods)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_mouseButtonHandlers) {
        handler(button, action, mods);
    }
}

void vku::Window::handleKey(GLFWwindow *raw, int key, int scancode, int action, int mods)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_keyHandlers) {
        handler(key, scancode, action, mods);
    }
}

void vku::Window::handleFocus(GLFWwindow *raw, int focused)
{
    vku::Window *window = (vku::Window *)glfwGetWindowUserPointer(raw);
    for (auto handler : window->_focusHandlers) {
        handler(focused);
    }
}

vku::Window::Window(GLFWwindow *raw, bool allowCursorDisable)
    : _raw(raw)
    , _allowCursorDisable(allowCursorDisable)
{
    count++;
    glfwSetWindowUserPointer(_raw, this);
    glfwSetCursorPosCallback(_raw, vku::Window::handleMouseMove);
    glfwSetMouseButtonCallback(_raw, vku::Window::handleMouseButton);
    glfwSetKeyCallback(_raw, vku::Window::handleKey);
    glfwSetWindowFocusCallback(_raw, vku::Window::handleFocus);
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
    , _allowCursorDisable(other._allowCursorDisable)
{
    glfwSetWindowUserPointer(_raw, this);
}

vku::Window &vku::Window::operator=(vku::Window &&other) noexcept
{
    if (this != &other) {
        std::swap(_raw, other._raw);
        glfwSetWindowUserPointer(_raw, this);
        _allowCursorDisable = other._allowCursorDisable;
    }
    return *this;
}

vku::Window vku::Window::noApi(int width, int height, const std::string &title, bool allowCursorDisable)
{
    if (count == 0 && glfwInit() == GLFW_FALSE) {
        throw std::runtime_error("GLFW failed to initialize");
    }
    glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
    auto raw = glfwCreateWindow(width, height, title.c_str(), nullptr, nullptr);
    return vku::Window(raw, allowCursorDisable);
}

void vku::Window::onMouseMove(std::function<void(double x, double y)> handler)
{
    _mouseMoveHandlers.push_back(handler);
}

void vku::Window::onMouseButton(std::function<void(int button, int action, int mods)> handler)
{
    _mouseButtonHandlers.push_back(handler);
}

void vku::Window::onKey(std::function<void(int key, int scancode, int action, int mods)> handler)
{
    _keyHandlers.push_back(handler);
}

void vku::Window::onFocus(std::function<void (int focused)> handler)
{
    _focusHandlers.push_back(handler);
}

glm::dvec2 vku::Window::mousePosition()
{
    glm::dvec2 pos;
    glfwGetCursorPos(_raw, &pos.x, &pos.y);
    return pos;
}

void vku::Window::disableCursor()
{
    if (_allowCursorDisable) {
        glfwSetInputMode(_raw, GLFW_CURSOR, GLFW_CURSOR_DISABLED);
    }
}

void vku::Window::resetCursor()
{
    glfwSetInputMode(_raw, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
}

int vku::Window::width()
{
    int value = -1;
    glfwGetWindowSize(_raw, &value, nullptr);
    return value;
}

int vku::Window::height()
{
    int value = -1;
    glfwGetWindowSize(_raw, nullptr, &value);
    return value;
}
