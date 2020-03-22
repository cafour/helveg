#include "render_pass.hpp"
#include "base.hpp"

#include <utility>

vku::RenderPass::RenderPass(VkDevice device, VkRenderPass raw)
    : _device(device)
    , _raw(raw)
{
}

vku::RenderPass::RenderPass(VkDevice device, VkRenderPassCreateInfo &createInfo)
    : _device(device)
{
    ENSURE(vkCreateRenderPass(_device, &createInfo, nullptr, &_raw));
}

vku::RenderPass::~RenderPass()
{
    if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
        vkDestroyRenderPass(_device, _raw, nullptr);
    }
}

vku::RenderPass::RenderPass(vku::RenderPass &&other) noexcept
    : _device(std::exchange(other._device, nullptr))
    , _raw(std::exchange(other._raw, nullptr))
{
}

vku::RenderPass &vku::RenderPass::operator=(vku::RenderPass &&other) noexcept
{
    if (this != &other) {
        std::swap(_device, other._device);
        std::swap(_raw, other._raw);
    }
    return *this;
}

static vku::RenderPass basic(VkDevice device, VkFormat colorFormat)
{
    VkAttachmentDescription colorAttachment = {};
    colorAttachment.format = colorFormat;
    colorAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
    colorAttachment.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
    colorAttachment.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
    colorAttachment.stencilLoadOp = VK_ATTACHMENT_LOAD_OP_DONT_CARE;
    colorAttachment.stencilStoreOp = VK_ATTACHMENT_STORE_OP_DONT_CARE;
    colorAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
    colorAttachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

    VkAttachmentReference colorAttachmentRef = {};
    colorAttachmentRef.attachment = 0;
    colorAttachmentRef.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

    VkSubpassDescription subpass = {};
    subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
    subpass.colorAttachmentCount = 1;
    subpass.pColorAttachments = &colorAttachmentRef;

    VkSubpassDependency dependency = {};
    dependency.srcSubpass = VK_SUBPASS_EXTERNAL;
    dependency.dstSubpass = 0;
    dependency.srcStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    dependency.srcAccessMask = 0;
    dependency.dstStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
    dependency.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_READ_BIT
        | VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;

    VkRenderPassCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
    createInfo.attachmentCount = 1;
    createInfo.pAttachments = &colorAttachment;
    createInfo.subpassCount = 1;
    createInfo.pSubpasses = &subpass;
    createInfo.dependencyCount = 1;
    createInfo.pDependencies = &dependency;

    return vku::RenderPass(device, createInfo);
}
