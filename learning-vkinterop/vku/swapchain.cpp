#include "swapchain.hpp"
#include "base.hpp"

void vku::Swapchain::initialize(Swapchain *old)
{
    auto capabilities = device().physicalDevice().surfaceCapabilities();
    _extent = capabilities.currentExtent;

    uint32_t imageCount = capabilities.minImageCount + 1;
    if (capabilities.maxImageCount > 0 && imageCount > capabilities.maxImageCount) {
        imageCount = capabilities.maxImageCount;
    }

    auto surfaceFormat = device().physicalDevice().surfaceFormat();

    VkSwapchainCreateInfoKHR createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = device().physicalDevice().surface();
    createInfo.minImageCount = imageCount;
    createInfo.imageFormat = surfaceFormat.format;
    createInfo.imageColorSpace = surfaceFormat.colorSpace;
    createInfo.imageExtent = capabilities.currentExtent;
    createInfo.imageArrayLayers = 1;
    createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
    createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    createInfo.preTransform = capabilities.currentTransform;
    createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    createInfo.presentMode = VK_PRESENT_MODE_FIFO_KHR;
    createInfo.clipped = VK_TRUE;
    createInfo.oldSwapchain = old != nullptr ? *old->raw() : VK_NULL_HANDLE;

    ENSURE(vkCreateSwapchainKHR(device(), &createInfo, nullptr, &_raw));

    if (old != nullptr) {
        old->~Swapchain();
    }

    ENSURE(vkGetSwapchainImagesKHR(device(), _raw, &imageCount, nullptr));
    _images.resize(imageCount);
    ENSURE(vkGetSwapchainImagesKHR(device(), _raw, &imageCount, _images.data()));

    _imageViews.resize(imageCount);
    for (size_t i = 0; i < imageCount; i++) {
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

    _framebuffers.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
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

    _fences.resize(imageCount, VK_NULL_HANDLE);
    _semaphores.resize(imageCount, VK_NULL_HANDLE);
}

vku::Swapchain::Swapchain(RenderPass &renderPass)
    : _renderPass(renderPass)
{
    initialize(nullptr);
}

vku::Swapchain::~Swapchain()
{
    for (auto fence : _fences) {
        if (fence != VK_NULL_HANDLE) {
            vkDestroyFence(device(), fence, nullptr);
        }
    }
    for (auto semaphore : _semaphores) {
        if (semaphore != VK_NULL_HANDLE) {
            vkDestroySemaphore(device(), semaphore, nullptr);
        }
    }
    for (auto semaphore : _recycledSemaphores) {
        vkDestroySemaphore(device(), semaphore, nullptr);
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
        VkSemaphoreCreateInfo createInfo = {VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO};
        ENSURE(vkCreateSemaphore(device(), &createInfo, nullptr, &acquireSemaphore));
    } else {
        acquireSemaphore = _recycledSemaphores.back();
        _recycledSemaphores.pop_back();
    }

    VkResult result = vkAcquireNextImageKHR(
        renderPass().device(),
        _raw,
        UINT64_MAX,
        acquireSemaphore, // the semaphore is signalled when the swapchain is done reading
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

    frame.index = img;
    frame.acquireSemaphore = acquireSemaphore;
    frame.releaseSemaphore = _semaphores[img];
    frame.fence = _fences[img];
    return VK_SUCCESS;
}

void vku::Swapchain::rebuild()
{
    initialize(this);
}
