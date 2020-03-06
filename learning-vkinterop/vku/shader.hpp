#pragma once

#include <string>
#include <volk.h>

#include "device.hpp"

namespace vku {

class Shader {
private:
    VkShaderModule _raw;
    Device &_device;

public:
    Shader(Device &device, const uint32_t *code, size_t size);
    Shader(Device &device, const char *filename);
    ~Shader();

    Device &device() { return _device; }

    operator VkShaderModule() { return _raw; };
};
}
