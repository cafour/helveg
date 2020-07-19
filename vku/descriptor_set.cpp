#include "descriptor_set.hpp"

VkDescriptorSetLayoutBinding vku::descriptorBinding(
    uint32_t binding,
    VkDescriptorType descriptorType,
    uint32_t descriptorCount,
    VkShaderStageFlags stageFlags,
    const VkSampler *pImmutableSamplers)
{
    VkDescriptorSetLayoutBinding raw = {};
    raw.binding = binding;
    raw.descriptorType = descriptorType;
    raw.descriptorCount = descriptorCount;
    raw.stageFlags = stageFlags;
    raw.pImmutableSamplers = pImmutableSamplers;
    return raw;
}

VkDescriptorPoolSize vku::descriptorPoolSize(VkDescriptorType type, size_t descriptorCount)
{
    VkDescriptorPoolSize size = {};
    size.descriptorCount = static_cast<uint32_t>(descriptorCount);
    size.type = type;
    return size;
}

std::vector<VkDescriptorSet> vku::allocateDescriptorSets(
    VkDevice device,
    VkDescriptorPool descriptorPool,
    VkDescriptorSetLayout setLayout,
    size_t count)
{
    std::vector<VkDescriptorSetLayout> layouts(count, setLayout);
    VkDescriptorSetAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
    allocateInfo.pSetLayouts = layouts.data();
    allocateInfo.descriptorSetCount = static_cast<uint32_t>(layouts.size());
    allocateInfo.descriptorPool = descriptorPool;

    std::vector<VkDescriptorSet> sets(count);
    ENSURE(vkAllocateDescriptorSets(device, &allocateInfo, sets.data()));
    return sets;
}

void vku::writeImageDescriptor(
    VkDevice device,
    VkDescriptorSet descriptorSet,
    VkDescriptorType descriptorType,
    VkDescriptorImageInfo *imageDescriptorInfos,
    uint32_t binding,
    uint32_t descriptorCount)
{
    VkWriteDescriptorSet write {};
    write.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
    write.dstSet = descriptorSet;
    write.descriptorType = descriptorType;
    write.dstBinding = binding;
    write.pImageInfo = imageDescriptorInfos;
    write.descriptorCount = descriptorCount;
    vkUpdateDescriptorSets(device, 1, &write, 0, nullptr);
}

void vku::writeWholeBufferDescriptor(
    VkDevice device,
    VkDescriptorType descriptorType,
    VkBuffer buffer,
    VkDescriptorSet descriptorSet,
    uint32_t binding)
{
    VkDescriptorBufferInfo bufferInfo = {};
    bufferInfo.buffer = buffer;
    bufferInfo.offset = 0;
    bufferInfo.range = VK_WHOLE_SIZE;

    VkWriteDescriptorSet write = {};
    write.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
    write.dstSet = descriptorSet;
    write.dstBinding = binding;
    write.dstArrayElement = 0;
    write.descriptorType = descriptorType;
    write.descriptorCount = 1;
    write.pBufferInfo = &bufferInfo;
    vkUpdateDescriptorSets(device, 1, &write, 0, nullptr);
}

vku::DescriptorSetLayout vku::DescriptorSetLayout::basic(
    VkDevice device,
    const VkDescriptorSetLayoutBinding *bindings,
    size_t bindingCount)
{
    VkDescriptorSetLayoutCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
    createInfo.bindingCount = static_cast<uint32_t>(bindingCount);
    createInfo.pBindings = bindings;
    return vku::DescriptorSetLayout(device, createInfo);
}

vku::DescriptorPool vku::DescriptorPool::basic(
    VkDevice device,
    size_t maxSets,
    const VkDescriptorPoolSize *sizes,
    size_t sizeCount)
{
    VkDescriptorPoolCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
    createInfo.maxSets = static_cast<uint32_t>(maxSets);
    createInfo.poolSizeCount = static_cast<uint32_t>(sizeCount);
    createInfo.pPoolSizes = sizes;
    return vku::DescriptorPool(device, createInfo);
}
