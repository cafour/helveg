#include "swapchain.hpp"
#include "base.hpp"

vku::Swapchain::Swapchain(Device &device)
    : _device(device)
{
    auto &details = _device.physicalDevice().swapchainDetails();
    auto &indices = _device.physicalDevice().queueIndices();

    uint32_t imageCount = details.capabilities.minImageCount + 1;
    if (details.capabilities.maxImageCount > 0 && imageCount > details.capabilities.maxImageCount) {
        imageCount = details.capabilities.maxImageCount;
    }

    auto &window = _device.physicalDevice().surface().window();
    auto surfaceFormat = details.pickFormat();

    VkSwapchainCreateInfoKHR createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = _device.physicalDevice().surface();
    createInfo.minImageCount = imageCount;
    createInfo.imageFormat = surfaceFormat.format;
    createInfo.imageColorSpace = surfaceFormat.colorSpace;
    createInfo.imageExtent = details.pickExtent(window.width(), window.height());
    createInfo.imageArrayLayers = 1;
    createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

    uint32_t queueIndices[2] = { indices.graphics, indices.present };
    if (indices.graphics != indices.present) {
        createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
        createInfo.queueFamilyIndexCount = 2;
        createInfo.pQueueFamilyIndices = queueIndices;
    } else {
        createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    }
    createInfo.preTransform = details.capabilities.currentTransform;
    createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    createInfo.presentMode = details.pickPresentMode();
    createInfo.clipped = VK_TRUE;
    createInfo.oldSwapchain = nullptr;

    ENSURE(vkCreateSwapchainKHR, _device, &createInfo, nullptr, &_raw);
    ENSURE(vkGetSwapchainImagesKHR, _device, _raw, &imageCount, nullptr);
    _swapchainImages.resize(imageCount);
    ENSURE(vkGetSwapchainImagesKHR, _device, _raw, &imageCount, _swapchainImages.data());
}
