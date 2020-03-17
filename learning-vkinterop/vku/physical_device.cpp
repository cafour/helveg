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
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, nullptr));
    if (deviceCount == 0) {
        throw std::runtime_error("failed to find any GPU with Vulkan support");
    }

    std::vector<VkPhysicalDevice> devices(deviceCount);
    ENSURE(vkEnumeratePhysicalDevices(instance, &deviceCount, devices.data()));

    VkPhysicalDevice chosenDevice = VK_NULL_HANDLE;
    for (auto device : devices) {
        if (!hasExtensions(device, extensions, length)) {
            continue;
        }

        uint32_t queueFamilyCount = 0;
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);
        std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
        vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());
        vku::QueueIndices indices;
        for (uint32_t i = 0; i < queueFamilyCount && !indices.isComplete(); ++i) {
            VkBool32 isPresentSupported = false;
            vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &isPresentSupported);
            if (queueFamilies[0].queueFlags & VK_QUEUE_GRAPHICS_BIT && isPresentSupported) {
                _queueIndex = i;
                break;
            }
        }
        if (_queueIndex < 0) {
            continue;
        }
        chosenDevice = device;
    }
    if (chosenDevice == VK_NULL_HANDLE) {
        throw std::runtime_error("failed to find a suitable GPU");
    }
    _raw = chosenDevice;
}

VkSurfaceFormatKHR vku::PhysicalDevice::surfaceFormat()
{
    uint32_t formatCount;
    vkGetPhysicalDeviceSurfaceFormatsKHR(_raw, surface(), &formatCount, nullptr);
    std::vector<VkSurfaceFormatKHR> formats(formatCount);
    vkGetPhysicalDeviceSurfaceFormatsKHR(_raw, surface(), &formatCount, formats.data());

    VkSurfaceFormatKHR chosenFormat;
    if (formatCount == 1 && formats[0].format == VK_FORMAT_UNDEFINED) {
        chosenFormat = formats[0];
        chosenFormat.format = VK_FORMAT_B8G8R8A8_UNORM;
        return chosenFormat;
    }

    if (formatCount == 0) {
        throw std::runtime_error("surface has no formats");
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

VkSurfaceCapabilitiesKHR vku::PhysicalDevice::surfaceCapabilities()
{
    VkSurfaceCapabilitiesKHR surfaceCaps;
    ENSURE(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(_raw, surface(), &surfaceCaps));
    return surfaceCaps;
}
