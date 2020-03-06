#pragma once

#include <stdexcept>

// volk needs to be before glfw
#include <volk.h>
#include <GLFW/glfw3.h>

namespace vku {
class Window {
private:
    static size_t _count;
    GLFWwindow *_raw;
    int _width;
    int _height;
    void (*_onResize)(void *userData, int width, int height);
    void *_userData;

    static void resizeCallback(GLFWwindow *glfwWindow, int width, int height);

public:
    Window(
        int width,
        int height,
        const char *title,
        void (*onResize)(void *userData, int width, int height) = nullptr,
        void *userData = nullptr);

    ~Window();

    operator GLFWwindow *() { return _raw; }

    int width() const { return _width; }
    int height() const { return _height; }
};
}
