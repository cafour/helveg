#pragma once

// volk needs to be before glfw
#include <volk.h>
#include <GLFW/glfw3.h>

#include <stdexcept>
#include <string>

namespace vku {
class Window {
private:
    static size_t count;

    GLFWwindow *_raw;
    int _width;
    int _height;

public:
    Window(
        int width,
        int height,
        const std::string &title);

    ~Window();

    operator GLFWwindow *() { return _raw; }

    int width() const { return _width; }
    int height() const { return _height; }
};
}
