#include "image_view.hpp"
#include "base.hpp"

#include <utility>

vku::ImageView::ImageView(VkDevice device, VkImageView raw)
    : _device(device)
    , _raw(raw)
{
}

vku::ImageView::ImageView(VkDevice device, VkImageViewCreateInfo &createInfo)
    : _device(device)
{
    ENSURE(vkCreateImageView(_device, &createInfo, nullptr, &_raw));
}

vku::ImageView::~ImageView()
{
    if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
        vkDestroyImageView(_device, _raw, nullptr);
    }
}

vku::ImageView::ImageView(vku::ImageView &&other) noexcept
    : _device(std::exchange(other._device, nullptr))
    , _raw(std::exchange(other._raw, nullptr))
{
}

vku::ImageView &vku::ImageView::operator=(vku::ImageView &&other) noexcept
{
    if (this != &other) {
        std::swap(_device, other._device);
        std::swap(_raw, other._raw);
    }
    return *this;
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
