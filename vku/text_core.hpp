#pragma once

#include "wrapper.hpp"

/**
 * The following class is largely based on a sample by Sascha Willems (https://github.com/SaschaWillems/Vulkan),
 * which is licensed under the MIT license.
 **/

namespace vku {
class RenderCore;

class TextCore {
private:
    vku::RenderCore &_render;
    VkBuffer _modelBuffer;
    VkBuffer _cameraBuffer;
    VkRenderPass _renderPass;

    vku::Sampler _fontSampler;
    vku::Image _fontImage;
    vku::ImageView _fontImageView;
    vku::DeviceMemory _fontImageMemory;

    vku::DescriptorSetLayout _textDSL;
    vku::DescriptorPool _textDP;
    VkDescriptorSet _textDS;
    vku::PipelineLayout _textPL;
    vku::GraphicsPipeline _textGP;

    void createFontImage();
    void createTextGP();

public:
    TextCore(vku::RenderCore &render, VkBuffer cameraBuffer);
};
}
