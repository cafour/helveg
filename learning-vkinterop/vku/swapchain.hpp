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
    VkExtent2D _extent;

public:
    Swapchain(RenderPass &renderPass);
    ~Swapchain();
    Swapchain(const Swapchain &other)
        : Swapchain(other._renderPass)
    {
    }
    Swapchain(Swapchain &&other) noexcept
        : _raw(other._raw)
        , _renderPass(other._renderPass)
        , _images(std::move(other._images))
        , _imageViews(std::move(other._imageViews))
        , _framebuffers(std::move(other._framebuffers))
        , _extent(other._extent)
    {
    }
    Swapchain &operator=(const Swapchain &other)
    {
        return *this = Swapchain(other);
    }
    Swapchain &operator=(Swapchain &&other) noexcept
    {
        return *this = Swapchain(other);
    }

    operator VkSwapchainKHR() { return _raw; }

    RenderPass &renderPass() { return _renderPass; }
    std::vector<VkImage> &images() { return _images; }
    std::vector<VkImageView> &imageViews() { return _imageViews; }
    std::vector<VkFramebuffer> &framebuffers() { return _framebuffers; }
    VkExtent2D extent() { return _extent; }
};
}
