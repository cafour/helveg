#include "device.hpp"
#include "base.hpp"

#include <set>
#include <vector>

vku::Device::Device(
    PhysicalDevice &physicalDevice,
    const char *const *extensions,
    size_t extensionCount)
    : _physicalDevice(physicalDevice)
{
    for (size_t i = 0; i < extensionCount; ++i) {
        _extensions.emplace_back(extensions[i]);
    }
    _extensionPtrs.resize(_extensions.size());
    for (size_t i = 0; i < _extensions.size(); ++i) {
        _extensionPtrs[i] = _extensions[i].c_str();
    }

    float queuePriority = 1.0f;
    VkDeviceQueueCreateInfo queueCreateInfo = {};
    queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
    queueCreateInfo.queueFamilyIndex = physicalDevice.queueIndex();
    queueCreateInfo.queueCount = 1;
    queueCreateInfo.pQueuePriorities = &queuePriority;

    VkPhysicalDeviceFeatures features = {};

    VkDeviceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    createInfo.pQueueCreateInfos = &queueCreateInfo;
    createInfo.queueCreateInfoCount = 1;
    createInfo.pEnabledFeatures = &features;
    createInfo.enabledExtensionCount = static_cast<uint32_t>(_extensionPtrs.size());
    createInfo.ppEnabledExtensionNames = _extensionPtrs.data();
    createInfo.enabledLayerCount = static_cast<uint32_t>(_physicalDevice.instance().layers().size());
    createInfo.ppEnabledLayerNames = _physicalDevice.instance().layers().data();

    ENSURE(vkCreateDevice(physicalDevice, &createInfo, nullptr, &_raw));
}
