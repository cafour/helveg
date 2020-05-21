#pragma once

#include "render_core.hpp"

namespace vku {
class DepthCore {
private:
    vku::DisplayCore &_displayCore;
    VkFormat _depthFormat;
    vku::Image _depthImage;
    vku::DeviceMemory _depthImageMemory;
    vku::ImageView _depthImageView;

    void onResize(size_t imageCount, VkExtent2D extent);

public:
    DepthCore(vku::DisplayCore &displayCore, vku::RenderCore &renderCore);

    VkFormat depthFormat() { return _depthFormat; }
    vku::Image &depthImage() { return _depthImage; }
    vku::DeviceMemory &depthImageMemory() { return _depthImageMemory; }
    vku::ImageView &depthImageView() { return _depthImageView; }
};
}
