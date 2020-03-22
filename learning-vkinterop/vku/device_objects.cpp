#include "device_objects.hpp"

vku::Semaphore vku::Semaphore::basic(VkDevice device)
{
    VkSemaphoreCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
    return vku::Semaphore(device, createInfo);
}

vku::Fence vku::Fence::basic(VkDevice device)
{
    VkFenceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
    createInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;
    return vku::Fence(device, createInfo);
}

vku::ImageView vku::ImageView::basic(VkDevice device, VkImage image, VkFormat format)
{
    VkImageViewCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    createInfo.image = image;
    createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    createInfo.format = format;
    createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    createInfo.subresourceRange.baseMipLevel = 0;
    createInfo.subresourceRange.levelCount = 1;
    createInfo.subresourceRange.baseArrayLayer = 0;
    createInfo.subresourceRange.layerCount = 1;
    return vku::ImageView(device, createInfo);
}
