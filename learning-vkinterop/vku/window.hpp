#pragma once

// volk needs to be before glfw
#include <GLFW/glfw3.h>
#include <volk.h>

#include <string>

namespace vku {
class Window {
private:
    static size_t count;

    GLFWwindow *_raw;

public:
    Window(GLFWwindow *raw);
    ~Window();
    Window(const Window &other) = delete;
    Window(Window &&other) = delete;
    Window &operator=(const Window &other) = delete;
    Window &operator=(Window &&other) = delete;

    operator GLFWwindow *() { return _raw; }
    GLFWwindow *raw() { return _raw; }

    static Window noApi(int width, int height, const std::string &title);
};
}
