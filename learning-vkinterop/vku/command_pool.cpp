#include "command_pool.hpp"

#include "base.hpp"

vku::CommandPool::CommandPool(vku::Device &device)
    : _device(device)
{
    VkCommandPoolCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    createInfo.queueFamilyIndex = device.physicalDevice().queueIndex();

    VkCommandPool commandPool;
    ENSURE(vkCreateCommandPool(device, &createInfo, nullptr, &_raw));
}

vku::CommandPool::~CommandPool()
{
    vkDestroyCommandPool(device(), _raw, nullptr);
}
