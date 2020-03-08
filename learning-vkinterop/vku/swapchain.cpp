#include "swapchain.hpp"
#include "base.hpp"

vku::Swapchain::Swapchain(RenderPass &renderPass)
    : _renderPass(renderPass)
{
    auto &details = renderPass.device().physicalDevice().swapchainDetails();
    auto &indices = renderPass.device().physicalDevice().queueIndices();

    uint32_t imageCount = details.capabilities.minImageCount + 1;
    if (details.capabilities.maxImageCount > 0 && imageCount > details.capabilities.maxImageCount) {
        imageCount = details.capabilities.maxImageCount;
    }

    auto &window = renderPass.device().physicalDevice().surface().window();
    auto surfaceFormat = details.pickFormat();
    _extent = details.pickExtent(window.width(), window.height());

    VkSwapchainCreateInfoKHR createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = renderPass.device().physicalDevice().surface();
    createInfo.minImageCount = imageCount;
    createInfo.imageFormat = surfaceFormat.format;
    createInfo.imageColorSpace = surfaceFormat.colorSpace;
    createInfo.imageExtent = _extent;
    createInfo.imageArrayLayers = 1;
    createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

    uint32_t queueIndices[2] = { indices.graphics, indices.present };
    if (indices.graphics != indices.present) {
        createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
        createInfo.queueFamilyIndexCount = 2;
        createInfo.pQueueFamilyIndices = queueIndices;
    } else {
        createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
    }
    createInfo.preTransform = details.capabilities.currentTransform;
    createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
    createInfo.presentMode = details.pickPresentMode();
    createInfo.clipped = VK_TRUE;
    createInfo.oldSwapchain = nullptr;

    ENSURE(vkCreateSwapchainKHR, renderPass.device(), &createInfo, nullptr, &_raw);
    ENSURE(vkGetSwapchainImagesKHR, renderPass.device(), _raw, &imageCount, nullptr);
    _images.resize(imageCount);
    ENSURE(vkGetSwapchainImagesKHR, renderPass.device(), _raw, &imageCount, _images.data());

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
        ENSURE(vkCreateImageView, renderPass.device(), &viewInfo, nullptr, &_imageViews[i]);
    }

    _framebuffers.resize(imageCount);
    for (size_t i = 0; i < imageCount; ++i) {
        VkFramebufferCreateInfo createInfo = {};
        createInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
        createInfo.renderPass = renderPass;
        createInfo.attachmentCount = 1;
        createInfo.pAttachments = &_imageViews[i];
        createInfo.width = _extent.width;
        createInfo.height = _extent.height;
        createInfo.layers = 1;

        ENSURE(vkCreateFramebuffer, renderPass.device(), &createInfo, nullptr, &_framebuffers[i]);
    }
}

vku::Swapchain::~Swapchain()
{
    for (auto framebuffer : _framebuffers) {
        vkDestroyFramebuffer(renderPass().device(), framebuffer, nullptr);
    }
    for (auto view : _imageViews) {
        vkDestroyImageView(renderPass().device(), view, nullptr);
    }
    vkDestroySwapchainKHR(renderPass().device(), _raw, nullptr);
}
