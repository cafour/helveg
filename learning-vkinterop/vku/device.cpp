#include "device.hpp"
#include "base.hpp"

#include <set>
#include <vector>

vku::Device::Device(
    PhysicalDevice &physicalDevice,
    const char **extensions,
    size_t extensionCount)
    : _physicalDevice(physicalDevice)
{
    std::vector<VkDeviceQueueCreateInfo> queueInfos;
    std::set<uint32_t> queueIndices = {
        physicalDevice.queueIndices().graphics,
        physicalDevice.queueIndices().present
    };
    float queuePriority = 1.0f;
    for (auto queueIndex : queueIndices) {
        VkDeviceQueueCreateInfo queueCreateInfo = {};
        queueCreateInfo.sType = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
        queueCreateInfo.queueFamilyIndex = queueIndex;
        queueCreateInfo.queueCount = 1;
        queueCreateInfo.pQueuePriorities = &queuePriority;
        queueInfos.push_back(queueCreateInfo);
    }

    for (size_t i = 0; i < extensionCount; ++i) {
        _extensions.emplace_back(extensions[i]);
    }
    _extensionPtrs.resize(_extensions.size());
    for (size_t i = 0; i < _extensions.size(); ++i) {
        _extensionPtrs[i] = _extensions[i].c_str();
    }

    VkPhysicalDeviceFeatures features = {};

    VkDeviceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
    createInfo.pQueueCreateInfos = queueInfos.data();
    createInfo.queueCreateInfoCount = static_cast<uint32_t>(queueInfos.size());
    createInfo.pEnabledFeatures = &features;
    createInfo.enabledExtensionCount = static_cast<uint32_t>(_extensionPtrs.size());
    createInfo.ppEnabledExtensionNames = _extensionPtrs.data();
    createInfo.enabledLayerCount = static_cast<uint32_t>(_physicalDevice.instance().layers().size());
    createInfo.ppEnabledLayerNames = _physicalDevice.instance().layers().data();

    ENSURE(vkCreateDevice, physicalDevice, &createInfo, nullptr, &_raw);
}
