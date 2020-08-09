#pragma once

#include "base.hpp"
#include "types.hpp"

#include <volk.h>

namespace vku {

class Semaphore : public DeviceConstructible<
                      VkSemaphore,
                      VkSemaphoreCreateInfo,
                      &vkCreateSemaphore,
                      &vkDestroySemaphore> {
public:
    using DeviceConstructible::DeviceConstructible;
    static Semaphore basic(VkDevice device);
};

class Fence : public DeviceConstructible<
                  VkFence,
                  VkFenceCreateInfo,
                  &vkCreateFence,
                  &vkDestroyFence> {
public:
    using DeviceConstructible::DeviceConstructible;
    static Fence basic(VkDevice device);
};

class Image : public DeviceConstructible<
                  VkImage,
                  VkImageCreateInfo,
                  &vkCreateImage,
                  &vkDestroyImage> {
public:
    using DeviceConstructible::DeviceConstructible;
    static Image basic(
        VkDevice device,
        VkExtent3D extent,
        VkFormat format,
        VkImageTiling tiling,
        VkImageUsageFlags usage);

    static VkImageCreateInfo createInfo()
    {
        VkImageCreateInfo createInfo = {};
        createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
        return createInfo;
    }
};

class ImageView : public DeviceConstructible<
                      VkImageView,
                      VkImageViewCreateInfo,
                      &vkCreateImageView,
                      &vkDestroyImageView> {
public:
    using DeviceConstructible::DeviceConstructible;
    static ImageView basic(VkDevice device, VkImage image, VkFormat format, VkImageAspectFlags aspectMask);

    static VkImageViewCreateInfo createInfo()
    {
        VkImageViewCreateInfo info = {};
        info.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
        return info;
    }
};

class Sampler : public DeviceConstructible<
                    VkSampler,
                    VkSamplerCreateInfo,
                    &vkCreateSampler,
                    &vkDestroySampler> {
public:
    using DeviceConstructible::DeviceConstructible;

    static VkSamplerCreateInfo createInfo()
    {
        VkSamplerCreateInfo info = {};
        info.sType = VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO;
        return info;
    }
};

class Swapchain : public DeviceConstructible<
                      VkSwapchainKHR,
                      VkSwapchainCreateInfoKHR,
                      &vkCreateSwapchainKHR,
                      &vkDestroySwapchainKHR> {
public:
    using DeviceConstructible::DeviceConstructible;
    static Swapchain
    basic(
        VkDevice device,
        VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface,
        VkSurfaceCapabilitiesKHR *pSurfaceCapabilities = nullptr,
        VkSurfaceFormatKHR *pSurfaceFormat = nullptr,
        VkSwapchainKHR old = VK_NULL_HANDLE);
};

class CommandPool : public DeviceConstructible<
                        VkCommandPool,
                        VkCommandPoolCreateInfo,
                        &vkCreateCommandPool,
                        &vkDestroyCommandPool> {
public:
    using DeviceConstructible::DeviceConstructible;
    static CommandPool basic(VkDevice device, uint32_t queueFamily);
};

class ShaderModule : public DeviceConstructible<
                         VkShaderModule,
                         VkShaderModuleCreateInfo,
                         &vkCreateShaderModule,
                         &vkDestroyShaderModule> {
public:
    using DeviceConstructible::DeviceConstructible;
    static ShaderModule inlined(VkDevice device, const uint32_t *code, size_t length);
    static ShaderModule fromFile(VkDevice device, const char *filename);
};

class Framebuffer : public DeviceConstructible<
                        VkFramebuffer,
                        VkFramebufferCreateInfo,
                        &vkCreateFramebuffer,
                        &vkDestroyFramebuffer> {
public:
    using DeviceConstructible::DeviceConstructible;
    static Framebuffer basic(
        VkDevice device,
        VkRenderPass renderPass,
        VkImageView *attachments,
        size_t attachmentCount,
        uint32_t width,
        uint32_t height);
};

class Buffer : public DeviceConstructible<
                   VkBuffer,
                   VkBufferCreateInfo,
                   &vkCreateBuffer,
                   &vkDestroyBuffer> {
public:
    using DeviceConstructible::DeviceConstructible;

    static Buffer exclusive(VkDevice device, size_t size, VkBufferUsageFlags usage);
};

class DeviceMemory : public DeviceConstructible<
                         VkDeviceMemory,
                         VkMemoryAllocateInfo,
                         &vkAllocateMemory,
                         &vkFreeMemory> {
public:
    using DeviceConstructible::DeviceConstructible;

    static DeviceMemory forBuffer(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkBuffer buffer,
        VkMemoryPropertyFlags requiredProperties,
        VkMemoryAllocateFlags flags = 0);

    static DeviceMemory forImage(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkImage image,
        VkMemoryPropertyFlags requiredProperties);

    static DeviceMemory hostCoherentBuffer(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkBuffer buffer);

    static DeviceMemory deviceLocalBuffer(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkBuffer buffer,
        VkMemoryAllocateFlags flags = 0);

    static DeviceMemory deviceLocalData(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkCommandPool copyPool,
        VkQueue transferQueue,
        VkBuffer buffer,
        const void *data,
        size_t dataSize,
        VkMemoryAllocateFlags flags = 0);
};

struct BackedBuffer {
    vku::Buffer buffer;
    vku::DeviceMemory memory;
};

vku::BackedBuffer stagingBuffer(
    VkPhysicalDevice physicalDevice,
    VkDevice device,
    size_t dataSize,
    const void *data = nullptr);

class AccelerationStructure : public DeviceConstructible<
                                  VkAccelerationStructureKHR,
                                  VkAccelerationStructureCreateInfoKHR,
                                  &vkCreateAccelerationStructureKHR,
                                  &vkDestroyAccelerationStructureKHR> {
public:
    using DeviceConstructible::DeviceConstructible;
};

}
