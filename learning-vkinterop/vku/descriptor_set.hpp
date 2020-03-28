#pragma once

#include "types.hpp"

#include <volk.h>

namespace vku {

VkDescriptorSetLayoutBinding descriptorBinding(
    uint32_t binding,
    VkDescriptorType descriptorType,
    uint32_t descriptorCount,
    VkShaderStageFlags stageFlags,
    const VkSampler *pImmutableSamplers = nullptr);

class DescriptorSetLayout : public DeviceConstructible<
                                VkDescriptorSetLayout,
                                VkDescriptorSetLayoutCreateInfo,
                                &vkCreateDescriptorSetLayout,
                                &vkDestroyDescriptorSetLayout> {
public:
    using DeviceConstructible::DeviceConstructible;
    static DescriptorSetLayout basic(
        VkDevice device,
        const VkDescriptorSetLayoutBinding *bindings,
        size_t bindingCount);
};

class DescriptorPool : public DeviceConstructible<
                           VkDescriptorPool,
                           VkDescriptorPoolCreateInfo,
                           &vkCreateDescriptorPool,
                           &vkDestroyDescriptorPool> {
public:
    using DeviceConstructible::DeviceConstructible;
};

}
