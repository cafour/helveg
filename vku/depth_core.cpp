#include "depth_core.hpp"

vku::DepthCore::DepthCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore)
    : _displayCore(displayCore)
{
    const std::vector<VkFormat> depthFormats {
        VK_FORMAT_D32_SFLOAT_S8_UINT,
        VK_FORMAT_D24_UNORM_S8_UINT
    };
    _depthFormat = vku::findSupportedFormat(
        _displayCore.physicalDevice(),
        depthFormats,
        VK_IMAGE_TILING_OPTIMAL,
        VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT);
    renderCore.onResize([this](auto s, auto e) { onResize(s, e); });
}

void vku::DepthCore::onResize(size_t, VkExtent2D extent)
{
    VkExtent3D depthExtent { extent.width, extent.height, 1 };
    _depthImage = vku::Image::basic(
        _displayCore.device(),
        depthExtent,
        _depthFormat,
        VK_IMAGE_TILING_OPTIMAL,
        VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT);
    _depthImageMemory = vku::DeviceMemory::forImage(
        _displayCore.physicalDevice(),
        _displayCore.device(),
        _depthImage,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
    _depthImageView = vku::ImageView::basic(
        _displayCore.device(),
        _depthImage,
        _depthFormat,
        VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT);
}
