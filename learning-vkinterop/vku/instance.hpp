#pragma once

#include <volk.h>

namespace vku {

class Instance {
private:
    VkInstance _raw;

public:
    Instance(VkInstance raw)
        : _raw(raw)
    {
    }

    Instance(const char *name, bool validate = false);
    ~Instance();

    operator VkInstance() { return _raw; }
};
}
