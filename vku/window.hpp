#pragma once

// volk needs to be before GLFW
#include <volk.h>

#include <GLFW/glfw3.h>

#include <string>

struct GLFWwindow;

namespace vku {
class Window {
private:
    static size_t count;

    GLFWwindow *_raw;

public:
    Window()
        : _raw(nullptr)
    {}
    Window(GLFWwindow *raw);
    ~Window();
    Window(const Window &other) = delete;
    Window(Window &&other) noexcept;
    Window &operator=(const Window &other) = delete;
    Window &operator=(Window &&other) noexcept;

    operator GLFWwindow *() { return _raw; }
    GLFWwindow *raw() { return _raw; }

    static Window noApi(int width, int height, const std::string &title);
};
}
