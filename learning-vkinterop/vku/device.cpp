#include "device.hpp"
#include "base.hpp"

#include <utility>
#include <vector>

vku::Device::Device(VkDevice raw)
    : _raw(raw)
{
}

vku::Device::Device(VkPhysicalDevice physicalDevice, VkDeviceCreateInfo &createInfo)
{
    ENSURE(vkCreateDevice(physicalDevice, &createInfo, nullptr, &_raw));
}

vku::Device::~Device()
{
    if (_raw != VK_NULL_HANDLE) {
        vkDestroyDevice(_raw, nullptr);
    }
}

vku::Device::Device(vku::Device &&other)
    : _raw(std::exchange(other._raw, VK_NULL_HANDLE))
{
}

vku::Device &vku::Device::operator=(vku::Device &&other)
{
    if (this != &other) {
        _raw = std::exchange(other._raw, VK_NULL_HANDLE);
    }
    return *this;
}

vku::Device vku::Device::basic(
    VkPhysicalDevice physicalDevice,
    uint32_t queueIndex,
    const std::vector<const char *> *extensions)
{
    float queuePriority = 1.0f;
    VkDeviceQueueCreateInfo queueCreateInfo = {};
    queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    queueCreateInfo.queueFamilyIndex = queueIndex;
    queueCreateInfo.queueCount = 1;
    queueCreateInfo.pQueuePriorities = &queuePriority;

    VkPhysicalDeviceFeatures features = {};

    VkDeviceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    createInfo.pQueueCreateInfos = &queueCreateInfo;
    createInfo.queueCreateInfoCount = 1;
    createInfo.pEnabledFeatures = &features;
    if (extensions) {
        createInfo.enabledExtensionCount = static_cast<uint32_t>(extensions->size());
        createInfo.ppEnabledExtensionNames = extensions->data();
    }

    return vku::Device(physicalDevice, createInfo);
}
