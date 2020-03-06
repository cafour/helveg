#pragma once

#include "window.hpp"
#include "instance.hpp"


namespace vku {

class Surface {
private:
    VkSurfaceKHR _raw;
    Instance &_instance;
    Window &_window;

public:
    Surface(Instance &instance, Window &window);
    ~Surface() { vkDestroySurfaceKHR(_instance, _raw, nullptr); }

    operator VkSurfaceKHR() { return _raw; }
};
}
