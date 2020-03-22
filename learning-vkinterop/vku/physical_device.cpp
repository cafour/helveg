#include "physical_device.hpp"
#include "base.hpp"
#include "capabilities.hpp"

#include <algorithm>
#include <cstring>
#include <vector>

bool vku::hasExtensionSupport(
    VkPhysicalDevice physicalDevice,
    const std::vector<const char *> &extensions)
{
    uint32_t extensionCount;
    vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, nullptr);

    std::vector<VkExtensionProperties> available(extensionCount);
    vkEnumerateDeviceExtensionProperties(physicalDevice, nullptr, &extensionCount, available.data());

    for (auto extensionName : extensions) {
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

VkPhysicalDevice vku::findDevice(
    VkInstance instance,
    VkSurfaceKHR surface,
    uint32_t *queueIndex,
    const std::vector<const char *> *requiredExtensions)
{
    uint32_t deviceCount = 0;
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, nullptr));
    if (deviceCount == 0) {
        throw std::runtime_error("failed to find any GPU with Vulkan support");
    }

    std::vector<VkPhysicalDevice> devices(deviceCount);
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, devices.data()));

    VkPhysicalDevice chosenDevice = VK_NULL_HANDLE;
    for (auto device : devices) {
        if (requiredExtensions && !hasExtensionSupport(device, *requiredExtensions)) {
            continue;
        }

        uint32_t queueFamilyCount = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);
        std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());

        for (uint32_t i = 0; i < queueFamilyCount; ++i) {
            VkBool32 isPresentSupported = false;
            vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &isPresentSupported);
            if (queueFamilies[0].queueFlags & VK_QUEUE_GRAPHICS_BIT && isPresentSupported) {
                *queueIndex = i;
                chosenDevice = device;
                break;
            }
        }
        if (chosenDevice != VK_NULL_HANDLE) {
            break;
        }
    }
    if (chosenDevice == VK_NULL_HANDLE) {
        throw std::runtime_error("failed to find a suitable physical device");
    }
    return chosenDevice;
}

VkSurfaceFormatKHR vku::findSurfaceFormat(
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface)
{
    uint32_t formatCount;
    vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, nullptr);
    std::vector<VkSurfaceFormatKHR> formats(formatCount);
    vkGetPhysicalDeviceSurfaceFormatsKHR(physicalDevice, surface, &formatCount, formats.data());

    VkSurfaceFormatKHR chosenFormat;
    if (formatCount == 1 && formats[0].format == VK_FORMAT_UNDEFINED) {
        chosenFormat = formats[0];
        chosenFormat.format = VK_FORMAT_B8G8R8A8_UNORM;
        return chosenFormat;
    }

    if (formatCount == 0) {
        throw std::runtime_error("failed to find a suitable surface format as surface has no formats");
    }

    chosenFormat.format = VK_FORMAT_UNDEFINED;
    for (auto &format : formats) {
        switch (format.format) {
        case VK_FORMAT_B8G8R8A8_UNORM:
            chosenFormat = format;
            break;

        default:
            break;
        }

        if (chosenFormat.format != VK_FORMAT_UNDEFINED) {
            break;
        }
    }

    if (chosenFormat.format == VK_FORMAT_UNDEFINED) {
        chosenFormat = formats[0];
    }

    return chosenFormat;
}
