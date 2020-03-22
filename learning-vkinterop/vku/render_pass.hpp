#pragma once

#include "device.hpp"

#include <volk.h>

namespace vku {

class RenderPass {
private:
    VkDevice _device;
    VkRenderPass _raw;

public:
    RenderPass(VkDevice device, VkRenderPass raw);
    RenderPass(VkDevice device, VkRenderPassCreateInfo &createInfo);
    ~RenderPass();
    RenderPass(const RenderPass &other) = delete;
    RenderPass(RenderPass &&other) noexcept;
    RenderPass &operator=(const RenderPass &other) = delete;
    RenderPass &operator=(RenderPass &&other) noexcept;

    operator VkRenderPass() { return _raw; }
    VkRenderPass raw() { return _raw; }
    VkDevice device() { return _device; }

    static RenderPass basic(VkDevice device, VkFormat colorFormat);
};
}
