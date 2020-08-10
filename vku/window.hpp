#pragma once

// volk needs to be before GLFW
#include <volk.h>

#include <GLFW/glfw3.h>
#include <glm/glm.hpp>

#include <string>
#include <functional>

struct GLFWwindow;

namespace vku {
class Window {
private:
    static size_t count;

    GLFWwindow *_raw;
    std::vector<std::function<void (double x, double y)>> _mouseMoveHandlers;
    std::vector<std::function<void (int button, int action, int mods)>> _mouseButtonHandlers;
    std::vector<std::function<void (int key, int scancode, int action, int mods)>> _keyHandlers;
    std::vector<std::function<void (int focused)>> _focusHandlers;

    static void handleMouseMove(GLFWwindow *raw, double x, double y);
    static void handleMouseButton(GLFWwindow *raw, int button, int action, int mods);
    static void handleKey(GLFWwindow *raw, int key, int scancode, int action, int mods);
    static void handleFocus(GLFWwindow *raw, int focused);

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

    static Window noApi(int width, int height, const std::string &title);

    void onMouseMove(std::function<void (double x, double y)> handler);
    void onMouseButton(std::function<void (int button, int action, int mods)> handler);
    void onKey(std::function<void (int key, int scancode, int action, int mods)> handler);
    void onFocus(std::function<void (int focused)> handler);
    glm::dvec2 mousePosition();
    void disableCursor();
    void resetCursor();
    int width();
    int height();

    operator GLFWwindow *() { return _raw; }
    GLFWwindow *raw() { return _raw; }

};
}
