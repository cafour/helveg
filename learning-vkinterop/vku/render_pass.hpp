#pragma once

#include "device.hpp"

#include <volk.h>

namespace vku {

class RenderPass {
private:
    VkRenderPass _raw;
    Device &_device;

public:
    RenderPass(Device &device);
    ~RenderPass() { vkDestroyRenderPass(_device, _raw, nullptr); }

    operator VkRenderPass() { return _raw; }

    Device &device() { return _device; }
};
}