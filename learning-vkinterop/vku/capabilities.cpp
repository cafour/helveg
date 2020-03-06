#include "capabilities.hpp"

VkSurfaceFormatKHR vku::SwapchainDetails::pickFormat()
{
    for (const auto &format : formats) {
        if (format.format == VK_FORMAT_B8G8R8A8_UNORM
            && format.colorSpace == VK_COLOR_SPACE_SRGB_NONLINEAR_KHR) {
            return format;
        }
    }
    return formats.front();
}

VkPresentModeKHR vku::SwapchainDetails::pickPresentMode()
{
    for (const auto &mode : presentModes) {
        if (mode == VK_PRESENT_MODE_MAILBOX_KHR) {
            return mode;
        }
    }
    return VK_PRESENT_MODE_FIFO_KHR;
}

VkExtent2D vku::SwapchainDetails::pickExtent(uint32_t width, uint32_t height)
{
    if (capabilities.currentExtent.width != UINT32_MAX) {
        return capabilities.currentExtent;
    }
    return {
        std::max(capabilities.minImageExtent.width, std::min(capabilities.maxImageExtent.width, width)),
        std::max(capabilities.minImageExtent.height, std::min(capabilities.maxImageExtent.height, height))
    };
}

vku::QueueIndices getQueueIndices(VkPhysicalDevice device, VkSurfaceKHR surface)
{
    uint32_t queueFamilyCount = 0;
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);
    std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
    vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());
    vku::QueueIndices indices;
    for (uint32_t i = 0; i < queueFamilyCount && !indices.isComplete(); ++i) {
        if (queueFamilies[0].queueFlags & VK_QUEUE_GRAPHICS_BIT) {
            indices.graphics = i;
        }
        VkBool32 isPresentSupported = false;
        vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &isPresentSupported);
        if (isPresentSupported) {
            indices.present = i;
        }
    }
    return indices;
}

vku::SwapchainDetails getSwapchainDetails(VkPhysicalDevice device, VkSurfaceKHR surface)
{
    vku::SwapchainDetails details;
    vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface, &details.capabilities);

    uint32_t formatCount;
    vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, nullptr);
    if (formatCount != 0) {
        details.formats.resize(formatCount);
        vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, details.formats.data());
    }

    uint32_t modeCount;
    vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &modeCount, nullptr);
    if (modeCount != 0) {
        details.presentModes.resize(modeCount);
        vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &modeCount, details.presentModes.data());
    }
    return details;
}
