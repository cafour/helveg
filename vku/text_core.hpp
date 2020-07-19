#pragma once

#include "wrapper.hpp"

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

    void createFontImage(vku::TransferCore &transfer);

public:
    TextCore(vku::TransferCore &transfer);
};
}
