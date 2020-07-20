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
    vku::Sampler _fontSampler;
    vku::Image _fontImage;
    vku::ImageView _fontImageView;
    vku::DeviceMemory _fontImageMemory;

    vku::Buffer _positionBuffer;
    vku::DeviceMemory _positionMemory;
    vku::Buffer _uvBuffer;
    vku::DeviceMemory _uvMemory;
    vku::Buffer _centerBuffer;
    vku::DeviceMemory _centerMemory;
    uint32_t _vertexCount;

    void createFontImage(vku::TransferCore &transfer);

public:
    TextCore(vku::TransferCore &transfer);

    void createBuffers(
        vku::TransferCore &transfer,
        const std::vector<std::string> &texts,
        const std::vector<glm::vec3> &textPositions,
        const std::vector<glm::vec2> &textScales);

    VkDescriptorImageInfo fontDescriptor();
    VkBuffer positionBuffer() { return _positionBuffer; }
    VkBuffer uvBuffer() { return _uvBuffer; }
    VkBuffer centerBuffer() { return _centerBuffer; }
    uint32_t vertexCount() { return _vertexCount; }
};
}
