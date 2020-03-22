#include "swapchain_env.hpp"
#include "base.hpp"

#include <vector>

vku::SwapchainEnv::SwapchainEnv(vku::Swapchain &&swapchain, VkRenderPass renderPass)
    : _swapchain(std::move(swapchain))
{
    auto test = vku::Semaphore::basic(swapchain.device());
    uint32_t imageCount = 0;
    ENSURE(vkGetSwapchainImagesKHR(_swapchain.device(), _swapchain, &imageCount, nullptr));
    std::vector<VkImage> images(imageCount, VK_NULL_HANDLE);
    ENSURE(vkGetSwapchainImagesKHR(_swapchain.device(), _swapchain, &imageCount, images.data()));

    
}

void vku::Swapchain::initialize(Swapchain *old)
{
    _imageViews.resize(_imageCount);
    for (size_t i = 0; i < _imageCount; i++) {
        VkImageViewCreateInfo viewInfo = {};
        viewInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        viewInfo.image = _images[i];
        viewInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
        viewInfo.format = surfaceFormat.format;
        viewInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
        viewInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
        viewInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
        viewInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
        viewInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
        viewInfo.subresourceRange.baseMipLevel = 0;
        viewInfo.subresourceRange.levelCount = 1;
        viewInfo.subresourceRange.baseArrayLayer = 0;
        viewInfo.subresourceRange.layerCount = 1;
        ENSURE(vkCreateImageView(device(), &viewInfo, nullptr, &_imageViews[i]));
    }

    _framebuffers.resize(_imageCount);
    for (size_t i = 0; i < _imageCount; ++i) {
        VkFramebufferCreateInfo createInfo = {};
        createInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
        createInfo.renderPass = renderPass();
        createInfo.attachmentCount = 1;
        createInfo.pAttachments = &_imageViews[i];
        createInfo.width = _extent.width;
        createInfo.height = _extent.height;
        createInfo.layers = 1;
        ENSURE(vkCreateFramebuffer(device(), &createInfo, nullptr, &_framebuffers[i]));
    }

    _fences.resize(_imageCount);
    for (size_t i = 0; i < _imageCount; ++i) {
        VkFenceCreateInfo fenceInfo = {};
        fenceInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
        fenceInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;
        ENSURE(vkCreateFence(device(), &fenceInfo, nullptr, &_fences[i]));
    }

    _releaseSemaphores.resize(_imageCount);
    for (size_t i = 0; i < _imageCount; ++i) {
        VkSemaphoreCreateInfo semaphoreInfo = {};
        semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
        ENSURE(vkCreateSemaphore(device(), &semaphoreInfo, nullptr, &_releaseSemaphores[i]));
    }

    _acquireSemaphores.resize(_imageCount);
}

vku::Swapchain::Swapchain(RenderPass &renderPass)
    : _renderPass(renderPass)
{
    initialize(nullptr);
}

vku::Swapchain::Swapchain(Swapchain &&other, bool shouldInitialize)
    : _renderPass(other._renderPass)
{
    if (!shouldInitialize) {
        std::swap(_raw, other._raw);
        std::swap(_images, other._images);
        std::swap(_imageViews, other._imageViews);
        std::swap(_framebuffers, other._framebuffers);
        std::swap(_releaseSemaphores, other._releaseSemaphores);
        std::swap(_recycledSemaphores, other._recycledSemaphores);
        std::swap(_fences, other._fences);
        std::swap(_extent, other._extent);
    } else {
        initialize(&other);
    }
}

vku::Swapchain::~Swapchain()
{
    for (size_t i = 0; i < _fences.size(); ++i) {
        if (_fences[i] != VK_NULL_HANDLE) {
            vkDestroyFence(device(), _fences[i], nullptr);
            _fences[i] = VK_NULL_HANDLE;
        }
    }
    while (!_recycledSemaphores.empty()) {
        vkDestroySemaphore(device(), _recycledSemaphores.back(), nullptr);
        _recycledSemaphores.pop_back();
    }
    for (size_t i = 0; i < _acquireSemaphores.size(); ++i) {
        if (_acquireSemaphores[i] != VK_NULL_HANDLE) {
            vkDestroySemaphore(device(), _acquireSemaphores[i], nullptr);
            _acquireSemaphores[i] = VK_NULL_HANDLE;
        }
    }
    for (size_t i = 0; i < _releaseSemaphores.size(); ++i) {
        if (_releaseSemaphores[i] != VK_NULL_HANDLE) {
            vkDestroySemaphore(device(), _releaseSemaphores[i], nullptr);
            _releaseSemaphores[i] = VK_NULL_HANDLE;
        }
    }
    for (auto framebuffer : _framebuffers) {
        vkDestroyFramebuffer(device(), framebuffer, nullptr);
    }
    for (auto view : _imageViews) {
        vkDestroyImageView(device(), view, nullptr);
    }
    vkDestroySwapchainKHR(device(), _raw, nullptr);
}

VkResult vku::Swapchain::acquire(SwapchainFrame &frame)
{
    uint32_t img;

    VkSemaphore acquireSemaphore;
    if (_recycledSemaphores.empty()) {
        VkSemaphoreCreateInfo info = {};
        info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
        ENSURE(vkCreateSemaphore(device(), &info, nullptr, &acquireSemaphore));
    } else {
        acquireSemaphore = _recycledSemaphores.back();
        _recycledSemaphores.pop_back();
    }

    VkResult result = vkAcquireNextImageKHR(
        renderPass().device(),
        _raw,
        UINT64_MAX,
        acquireSemaphore, // the semaphore is signaled when the swapchain is done reading
        VK_NULL_HANDLE,
        &img);
    if (result != VK_SUCCESS) {
        _recycledSemaphores.push_back(acquireSemaphore);
        return result;
    }

    // wait for the all command buffers to finish executing
    if (_fences[img] != VK_NULL_HANDLE) {
        ENSURE(vkWaitForFences(device(), 1, &_fences[img], true, UINT64_MAX));
        ENSURE(vkResetFences(device(), 1, &_fences[img]));
    }

    if (_acquireSemaphores[img] != VK_NULL_HANDLE) {
        _recycledSemaphores.push_back(_acquireSemaphores[img]);
    }
    _acquireSemaphores[img] = acquireSemaphore;

    frame.index = img;
    frame.acquireSemaphore = acquireSemaphore;
    frame.releaseSemaphore = _releaseSemaphores[img];
    frame.fence = _fences[img];
    return VK_SUCCESS;
}

void vku::Swapchain::rebuild()
{
    vkDeviceWaitIdle(device());
    *this = Swapchain(std::move(*this), true);
}
