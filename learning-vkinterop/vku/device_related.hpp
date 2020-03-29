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

class ImageView : public DeviceConstructible<
                      VkImageView,
                      VkImageViewCreateInfo,
                      &vkCreateImageView,
                      &vkDestroyImageView> {
public:
    using DeviceConstructible::DeviceConstructible;
    static ImageView basic(VkDevice device, VkImage image, VkFormat format);
};

class RenderPass : public DeviceConstructible<
                       VkRenderPass,
                       VkRenderPassCreateInfo,
                       &vkCreateRenderPass,
                       &vkDestroyRenderPass> {
public:
    using DeviceConstructible::DeviceConstructible;
    static vku::RenderPass basic(VkDevice device, VkFormat colorFormat);
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
    static CommandPool basic(VkDevice device, uint32_t queueIndex);
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
    static Framebuffer basic(VkDevice device, VkRenderPass renderPass, VkImageView imageView, uint32_t width, uint32_t height);
};

class Buffer : public DeviceConstructible<
                   VkBuffer,
                   VkBufferCreateInfo,
                   &vkCreateBuffer,
                   &vkDestroyBuffer> {
public:
    using DeviceConstructible::DeviceConstructible;

    static Buffer exclusive(VkDevice device, VkDeviceSize size, VkBufferUsageFlags usage);
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
        VkMemoryPropertyFlags requiredProperties = 0);

    static DeviceMemory host(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkBuffer buffer);

    static DeviceMemory deviceLocal(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkBuffer buffer);

    static DeviceMemory deviceLocalData(
        VkPhysicalDevice physicalDevice,
        VkDevice device,
        VkCommandPool copyPool,
        VkQueue transferQueue,
        VkBuffer buffer,
        const void *data,
        size_t dataSize);
};

}
