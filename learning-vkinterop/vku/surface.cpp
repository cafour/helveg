#include "surface.hpp"

#include "base.hpp"

vku::Surface::Surface(Instance &instance, Window &window)
    : _instance(instance)
    , _window(window)
{
    ENSURE(glfwCreateWindowSurface(_instance, _window, nullptr, &_raw));
}
