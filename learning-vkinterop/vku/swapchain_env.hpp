#pragma once

#include "render_pass.hpp"
#include "swapchain.hpp"
#include "semaphore.hpp"

#include <volk.h>

#include <functional>
#include <vector>

namespace vku {

struct SwapchainFrame {
    uint32_t index;
    Semaphore &acquireSem;
    VkSemaphore acquireSemaphore;
    VkSemaphore releaseSemaphore;
    VkFence fence;
};

class SwapchainEnv {
private:
    Swapchain _swapchain;
    std::reference_wrapper<RenderPass> _renderPass;
    std::vector<VkImage> _images;
    std::vector<VkImageView> _imageViews;
    std::vector<VkFramebuffer> _framebuffers;
    std::vector<VkSemaphore> _acquireSemaphores;
    std::vector<VkSemaphore> _releaseSemaphores;
    std::vector<VkSemaphore> _recycledSemaphores;
    std::vector<VkFence> _fences;
    VkExtent2D _extent;
    uint32_t _imageCount;

public:
    SwapchainEnv(Swapchain &&swapchain, VkRenderPass renderPass)
    Swapchain(RenderPass &renderPass);
    ~Swapchain();
    Swapchain(const Swapchain &other) = delete;
    Swapchain(Swapchain &&other, bool shouldInitialize = false);
    Swapchain &operator=(const Swapchain &other) = delete;
    Swapchain &operator=(Swapchain &&other) noexcept
    {
        std::swap(_raw, other._raw);
        std::swap(_renderPass, other._renderPass);
        std::swap(_images, other._images);
        std::swap(_imageViews, other._imageViews);
        std::swap(_framebuffers, other._framebuffers);
        std::swap(_acquireSemaphores, other._acquireSemaphores);
        std::swap(_releaseSemaphores, other._releaseSemaphores);
        std::swap(_recycledSemaphores, other._recycledSemaphores);
        std::swap(_fences, other._fences);
        std::swap(_extent, other._extent);
        return *this;
    }

    operator VkSwapchainKHR() { return _raw; }
    VkSwapchainKHR *raw() { return &_raw; }

    RenderPass &renderPass() { return _renderPass; }
    Device &device() { return renderPass().device(); }
    std::vector<VkImage> &images() { return _images; }
    std::vector<VkImageView> &imageViews() { return _imageViews; }
    std::vector<VkFramebuffer> &framebuffers() { return _framebuffers; }
    uint32_t imageCount() { return _imageCount; }
    VkExtent2D extent() { return _extent; }
    VkResult acquire(SwapchainFrame &frame);
    void rebuild();
};
}