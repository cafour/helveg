#pragma once

#include <stdexcept>

#include <GLFW/glfw3.h>

namespace vku {
class Window {
private:
    static size_t _count;
    GLFWwindow *_window;
    int _width;
    int _height;
    void (*_onResize)(void *userData, int width, int height);
    void *_userData;

    static friend void resizeCallback(GLFWwindow *window, int width, int height)
    {
        auto window = reinterpret_cast<Window *>(glfwGetWindowUserPointer(window));
        window->_onResize(window->userData, width, height);
    }

public:
    Window(int width, int height, const char *title, void (*onResize)(void *userData, int width, int height) = nullptr, void *userData = nullptr)
        : _width(width)
        , _height(height)
        , _onResize(onResize)
        , _userData(userData) {
        if (_count == 0 && glfwInit() == GLFW_FALSE) {
            throw std::runtime_error("GLFW failed to initialize");
        }
        _count++;
        glfwWindowHint(GLFW_CLIENT_API, GLFW_NO_API);
        _window = glfwCreateWindow(width, height, title, nullptr, nullptr);
        glfwSetFramebufferSizeCallback(_window, resizeCallback);
    }

    ~Window() {
        glfwDestroyWindow(_window);
        _count--;
        if (_count == 0) {
            glfwTerminate();
        }
    }

    int width() const { return _width; }
    int height() const { return _height; }
};
}
