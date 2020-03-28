#pragma once

#include "types.hpp"

#include <volk.h>

namespace vku {

class DescriptorSetLayout : public DeviceConstructible<
                                VkDescriptorSetLayout,
                                VkDescriptorSetLayoutCreateInfo,
                                &vkCreateDescriptorSetLayout,
                                &vkDestroyDescriptorSetLayout> {
public:
    using DeviceConstructible::DeviceConstructible;
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
