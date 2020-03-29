#include "device_related.hpp"

#include <cstring>
#include <fstream>
#include <stdexcept>

vku::Semaphore vku::Semaphore::basic(VkDevice device)
{
    VkSemaphoreCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
    return vku::Semaphore(device, createInfo);
}

vku::Fence vku::Fence::basic(VkDevice device)
{
    VkFenceCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
    createInfo.flags = VK_FENCE_CREATE_SIGNALED_BIT;
    return vku::Fence(device, createInfo);
}

vku::ImageView vku::ImageView::basic(VkDevice device, VkImage image, VkFormat format)
{
    VkImageViewCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
    createInfo.image = image;
    createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
    createInfo.format = format;
    createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
    createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
    createInfo.subresourceRange.baseMipLevel = 0;
    createInfo.subresourceRange.levelCount = 1;
    createInfo.subresourceRange.baseArrayLayer = 0;
    createInfo.subresourceRange.layerCount = 1;
    return vku::ImageView(device, createInfo);
}

vku::RenderPass vku::RenderPass::basic(VkDevice device, VkFormat colorFormat)
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

vku::Swapchain vku::Swapchain::basic(
    VkDevice device,
    VkPhysicalDevice physicalDevice,
    VkSurfaceKHR surface,
    VkSurfaceCapabilitiesKHR *pSurfaceCapabilities,
    VkSurfaceFormatKHR *pSurfaceFormat,
    VkSwapchainKHR old)
{
    ENSURE(vkDeviceWaitIdle(device));

    VkSurfaceCapabilitiesKHR capabilities;
    ENSURE(vkGetPhysicalDeviceSurfaceCapabilitiesKHR(physicalDevice, surface, &capabilities));
    if (pSurfaceCapabilities) {
        *pSurfaceCapabilities = capabilities;
    }

    uint32_t imageCount = capabilities.minImageCount + 1;
    if (capabilities.maxImageCount > 0 && imageCount > capabilities.maxImageCount) {
        imageCount = capabilities.maxImageCount;
    }

    auto surfaceFormat = vku::findSurfaceFormat(physicalDevice, surface);
    if (pSurfaceFormat) {
        *pSurfaceFormat = surfaceFormat;
    }

    VkSwapchainCreateInfoKHR createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
    createInfo.surface = surface;
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
    createInfo.oldSwapchain = old;
    return vku::Swapchain(device, createInfo);
}

vku::CommandPool vku::CommandPool::basic(VkDevice device, uint32_t queueIndex)
{
    VkCommandPoolCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
    createInfo.queueFamilyIndex = queueIndex;
    return vku::CommandPool(device, createInfo);
}

vku::ShaderModule vku::ShaderModule::inlined(VkDevice device, const uint32_t *code, size_t size)
{
    VkShaderModuleCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
    createInfo.codeSize = size;
    createInfo.pCode = code;
    return vku::ShaderModule(device, createInfo);
}

vku::ShaderModule vku::ShaderModule::fromFile(VkDevice device, const char *filename)
{
    std::ifstream file(filename, std::ios::ate | std::ios::binary);
    if (!file.is_open()) {
        throw std::runtime_error("failed to open a file");
    }

    size_t size = (size_t)file.tellg();
    file.seekg(0);
    std::vector<char> buffer(size);
    file.read(buffer.data(), size);
    file.close();

    return inlined(device, reinterpret_cast<const uint32_t *>(buffer.data()), size);
}

vku::Framebuffer vku::Framebuffer::basic(
    VkDevice device,
    VkRenderPass renderPass,
    VkImageView imageView,
    uint32_t width,
    uint32_t height)
{
    VkFramebufferCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
    createInfo.renderPass = renderPass;
    createInfo.attachmentCount = 1;
    createInfo.pAttachments = &imageView;
    createInfo.width = width;
    createInfo.height = height;
    createInfo.layers = 1;
    return vku::Framebuffer(device, createInfo);
}

vku::Buffer vku::Buffer::exclusive(VkDevice device, size_t size, VkBufferUsageFlags usage)
{
    VkBufferCreateInfo createInfo = {};
    createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
    createInfo.size = static_cast<VkDeviceSize>(size);
    createInfo.usage = usage;
    createInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
    return vku::Buffer(device, createInfo);
}

vku::DeviceMemory vku::DeviceMemory::forBuffer(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkBuffer buffer,
    VkMemoryPropertyFlags requiredProperties)
{
    VkMemoryRequirements memoryRequirements;
    vkGetBufferMemoryRequirements(device, buffer, &memoryRequirements);

    VkMemoryAllocateInfo allocateInfo = {};
    allocateInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
    allocateInfo.allocationSize = memoryRequirements.size;
    allocateInfo.memoryTypeIndex = vku::findMemoryType(
        physicalDevice,
        memoryRequirements.memoryTypeBits,
        requiredProperties);
    auto deviceMemory = vku::DeviceMemory(device, allocateInfo);
    ENSURE(vkBindBufferMemory(device, buffer, deviceMemory, 0));
    return deviceMemory;
}

vku::DeviceMemory vku::DeviceMemory::host(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkBuffer buffer)
{
    return vku::DeviceMemory::forBuffer(
        physicalDevice,
        device,
        buffer,
        VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
}

vku::DeviceMemory vku::DeviceMemory::deviceLocal(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkBuffer buffer)
{
    return vku::DeviceMemory::forBuffer(
        physicalDevice,
        device,
        buffer,
        VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
}

vku::DeviceMemory vku::DeviceMemory::deviceLocalData(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    VkCommandPool copyPool,
    VkQueue transferQueue,
    VkBuffer buffer,
    const void *data,
    size_t dataSize)
{
    auto stagingBuffer = vku::Buffer::exclusive(device, dataSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    auto stagingMemory = vku::DeviceMemory::host(physicalDevice, device, stagingBuffer);

    vku::hostDeviceCopy(device, data, stagingMemory, dataSize);

    auto memory = vku::DeviceMemory::deviceLocal(physicalDevice, device, buffer);
    vku::deviceDeviceCopy(device, copyPool, transferQueue, stagingBuffer, buffer, dataSize);
    return memory;
}
