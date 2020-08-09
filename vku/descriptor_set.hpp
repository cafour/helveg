#pragma once

#include "types.hpp"

#include <volk.h>

#include <vector>

namespace vku {

VkDescriptorSetLayoutBinding descriptorBinding(
    uint32_t binding,
    VkDescriptorType descriptorType,
    uint32_t descriptorCount,
    VkShaderStageFlags stageFlags,
    const VkSampler *pImmutableSamplers = nullptr);

std::vector<VkDescriptorSet> allocateDescriptorSets(
    VkDevice device,
    VkDescriptorPool descriptorPool,
    VkDescriptorSetLayout setLayout,
    size_t count);

void writeImageDescriptor(
    VkDevice device,
    VkDescriptorSet descriptorSet,
    VkDescriptorType descriptorType,
    VkDescriptorImageInfo *imageDescriptorInfos,
    uint32_t binding,
    uint32_t descriptorCount = 1);

void writeWholeBufferDescriptor(
    VkDevice device,
    VkDescriptorType descriptorType,
    VkBuffer buffer,
    VkDescriptorSet descriptorSet,
    uint32_t binding);

void writeASDescriptor(
    VkDevice device,
    VkDescriptorSet descriptorSet,
    VkAccelerationStructureKHR as,
    uint32_t binding);

VkDescriptorPoolSize descriptorPoolSize(VkDescriptorType type, size_t descriptorCount);

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
    static DescriptorPool basic(
        VkDevice device,
        size_t maxSets,
        const VkDescriptorPoolSize *sizes,
        size_t sizeCount);
};

}
