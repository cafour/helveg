#pragma once

#include <volk.h>

namespace vku {

class Instance {
private:
    VkInstance _raw;
    VkDebugUtilsMessengerEXT _messenger = nullptr;

public:
    Instance(const char *name, bool validate = false);
    ~Instance()
    {
        if (_messenger) {
            vkDestroyDebugUtilsMessengerEXT(_raw, _messenger, nullptr);
        }
        vkDestroyInstance(_raw, nullptr);
    }

    operator VkInstance() { return _raw; }
};
}
