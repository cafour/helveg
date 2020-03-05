#include "shader.hpp"

#include "base.hpp"

vku::Shader::Shader(VkDevice device, const uint32_t *code, size_t size) {
        VkShaderModuleCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
    createInfo.codeSize = size;
    createInfo.pCode = code;

    ENSURE(vkCreateShaderModule, device, &createInfo, nullptr, &_raw);
}
