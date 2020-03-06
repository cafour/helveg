#pragma once

#include "render_pass.hpp"

#include <vector>
#include <volk.h>

namespace vku {

class Swapchain {
private:
    VkSwapchainKHR _raw;
    RenderPass &_renderPass;
    std::vector<VkImage> _images;
    std::vector<VkImageView> _imageViews;
    std::vector<VkFramebuffer> _framebuffers;

public:
    Swapchain(RenderPass &renderPass);
    ~Swapchain();

    operator VkSwapchainKHR() { return _raw; }

    RenderPass &renderPass() { return _renderPass; }
};
}
