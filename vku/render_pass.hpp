#pragma once

#include "types.hpp"

#include <volk.h>

namespace vku {

class RenderPass : public DeviceConstructible<
                       VkRenderPass,
                       VkRenderPassCreateInfo,
                       &vkCreateRenderPass,
                       &vkDestroyRenderPass> {
public:
    using DeviceConstructible::DeviceConstructible;
    static vku::RenderPass basic(
        VkDevice device,
        VkFormat colorFormat,
        VkFormat depthFormat = VK_FORMAT_UNDEFINED);
};
}
