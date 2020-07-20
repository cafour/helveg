#pragma once

#include "wrapper.hpp"

#include <string>
#include <vector>

/**
 * The following class is largely based on a sample by Sascha Willems (https://github.com/SaschaWillems/Vulkan),
 * which is licensed under the MIT license.
 **/

namespace vku {
class TransferCore;

class TextCore {
private:
    // VkRenderPass _renderPass;
    // VkBuffer _cameraBuffer;

    vku::Sampler _fontSampler;
    vku::Image _fontImage;
    vku::ImageView _fontImageView;
    vku::DeviceMemory _fontImageMemory;

    vku::Buffer _positionBuffer;
    vku::DeviceMemory _positionMemory;
    vku::Buffer _uvBuffer;
    vku::DeviceMemory _uvMemory;
    uint32_t _vertexCount;

    // vku::DescriptorSetLayout _textDSL;
    // vku::DescriptorPool _textDP;
    // VkDescriptorSet _textDS;
    // vku::PipelineLayout _textPL;
    // vku::GraphicsPipeline _textGP;

    void createFontImage(vku::TransferCore &transfer);
    // void createTextGP();

public:
    TextCore(
        vku::TransferCore &transfer,
        const std::vector<std::string> &texts,
        const std::vector<glm::vec3> &textPositions,
        const std::vector<glm::vec2> &textScales);

    VkDescriptorImageInfo fontDescriptor();
    VkBuffer positionBuffer() { return _positionBuffer; }
    VkBuffer uvBuffer() { return _uvBuffer; }
    uint32_t vertexCount() { return _vertexCount; }
};
}
