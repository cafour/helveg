#pragma once

#include <volk.h>

#include <string>

namespace vku {

class Shader {
private:
    VkDevice _device;
    VkShaderModule _raw;

public:
    Shader(VkShaderModule raw)
        : _raw(raw)
    {
    }

    Shader(VkDevice device, const uint32_t *code, size_t size);
    Shader(VkDevice device, const char *filename);
    ~Shader();

    operator VkShaderModule() { return _raw; };
};
}
