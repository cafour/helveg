#include "shader.hpp"
#include "base.hpp"

#include <fstream>
#include <vector>
#include <stdexcept>

vku::Shader::Shader(Device &device, const uint32_t *code, size_t size)
    : _device(device)
{
    VkShaderModuleCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
    createInfo.codeSize = size;
    createInfo.pCode = code;

    ENSURE(vkCreateShaderModule(device, &createInfo, nullptr, &_raw));
}

vku::Shader::Shader(Device &device, const char *filename)
    : _device(device)
{
    std::ifstream file(filename, std::ios::ate | std::ios::binary);
    if (!file.is_open()) {
        throw std::runtime_error("failed to open a file");
    }

    size_t size = (size_t)file.tellg();
    file.seekg(0);
    std::vector<char> buffer(size);
    file.read(buffer.data(), size);
    file.close();

    vku::Shader(device, reinterpret_cast<const uint32_t *>(buffer.data()), size);
}

vku::Shader::~Shader()
{
    vkDestroyShaderModule(_device, _raw, nullptr);
}
