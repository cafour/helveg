#include "swapchain.hpp"
#include "base.hpp"
#include "physical_device.hpp"

#include <utility>

vku::Swapchain::Swapchain(VkDevice device, VkSwapchainKHR raw)
    : _device(device)
    , _raw(raw)
{
}

vku::Swapchain::Swapchain(VkDevice device, VkSwapchainCreateInfoKHR &createInfo)
    : _device(device)
{
    ENSURE(vkCreateSwapchainKHR(_device, &createInfo, nullptr, &_raw));
}

vku::Swapchain::~Swapchain()
{
    if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
        vkDestroySwapchainKHR(_device, _raw, nullptr);
    }
}

vku::Swapchain::Swapchain(vku::Swapchain &&other)
    : _device(std::exchange(other._device, nullptr))
    , _raw(std::exchange(other._raw, nullptr))
{
}

vku::Swapchain &vku::Swapchain::operator=(vku::Swapchain &&other)
{
    if (this != &other) {
        std::swap(_device, other._device);
        std::swap(_raw, other._raw);
    }
    return *this;
}

vku::Swapchain vku::Swapchain::basic(
    VkDevice device,
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface,
    VkSwapchainKHR old = VK_NULL_HANDLE)
{
    VkSurfaceCapabilitiesKHR capabilities;
    ENSURE(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(physicalDevice, surface, &capabilities));

    uint32_t imageCount = capabilities.minImageCount + 1;
    if (capabilities.maxImageCount > 0 && imageCount > capabilities.maxImageCount) {
        imageCount = capabilities.maxImageCount;
    }

    auto surfaceFormat = vku::findSurfaceFormat(physicalDevice, surface);

    VkSwapchainCreateInfoKHR createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = surface;
    createInfo.minImageCount = imageCount;
    createInfo.imageFormat = surfaceFormat.format;
    createInfo.imageColorSpace = surfaceFormat.colorSpace;
    createInfo.imageExtent = capabilities.currentExtent;
    createInfo.imageArrayLayers = 1;
    createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
    createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    createInfo.preTransform = capabilities.currentTransform;
    createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    createInfo.presentMode = VK_PRESENT_MODE_FIFO_KHR;
    createInfo.clipped = VK_TRUE;
    createInfo.oldSwapchain = old;

    return vku::Swapchain(device, createInfo);
}
