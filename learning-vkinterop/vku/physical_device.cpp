#include "physical_device.hpp"
#include "base.hpp"
#include "capabilities.hpp"

#include <algorithm>
#include <cstring>
#include <vector>

static bool hasExtensions(VkPhysicalDevice device, const char **extensions, size_t length)
{
    uint32_t extensionCount;
    vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, nullptr);

    std::vector<VkExtensionProperties> available(extensionCount);
    vkEnumerateDeviceExtensionProperties(device, nullptr, &extensionCount, available.data());

    for (size_t i = 0; i < length; ++i) {
        const char *extensionName = extensions[i];
        bool containsExtension = std::any_of(available.begin(), available.end(),
            [extensionName](const auto &extension) {
                return !strcmp(extensionName, extension.extensionName);
            });
        if (!containsExtension) {
            return false;
        }
    }
    return true;
}

vku::PhysicalDevice::PhysicalDevice(
    Instance &instance,
    Surface &surface,
    const char **extensions,
    size_t length)
    : _instance(instance)
    , _surface(surface)
{
    uint32_t deviceCount = 0;
    ENSURE(vkEnumeratePhysicalDevices, instance, &deviceCount, nullptr);
    if (deviceCount == 0) {
        throw std::runtime_error("failed to find any GPU with Vulkan support");
    }

    std::vector<VkPhysicalDevice> devices(deviceCount);
    ENSURE(vkEnumeratePhysicalDevices, instance, &deviceCount, devices.data());

    VkPhysicalDevice chosen = VK_NULL_HANDLE;
    for (auto device : devices) {
        if (!hasExtensions(device, extensions, length)) {
            continue;
        }
        _swapchainDetails = getSwapchainDetails(device, surface);
        if (_swapchainDetails.formats.empty() || _swapchainDetails.presentModes.empty()) {
            continue;
        }
        _queueIndices = getQueueIndices(device, surface);
        if (!_queueIndices.isComplete()) {
            continue;
        }
        chosen = device;
    }
    if (chosen == VK_NULL_HANDLE) {
        throw std::runtime_error("failed to find a suitable VkPhysicalDevice");
    }
    _raw = chosen;
}
