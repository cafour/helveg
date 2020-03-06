#include "physical_device.hpp"

#include <vector>
#include <algorithm>
#include <cstring>

bool vku::PhysicalDevice::hasExtensions(VkPhysicalDevice device, const char **extensions, size_t length)
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
