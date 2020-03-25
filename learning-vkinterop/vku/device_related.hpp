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

class PipelineLayout : public DeviceConstructible<
                           VkPipelineLayout,
                           VkPipelineLayoutCreateInfo,
                           &vkCreatePipelineLayout,
                           &vkDestroyPipelineLayout> {
public:
    using DeviceConstructible::DeviceConstructible;
    static PipelineLayout basic(VkDevice device);
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

class GraphicsPipeline : public DeviceRelated<VkPipeline> {
public:
    using DeviceRelated::DeviceRelated;

    GraphicsPipeline(VkDevice device, VkGraphicsPipelineCreateInfo &createInfo)
        : DeviceRelated(device, VK_NULL_HANDLE)
    {
        ENSURE(vkCreateGraphicsPipelines(device, VK_NULL_HANDLE, 1, &createInfo, nullptr, &_raw));
    }
    ~GraphicsPipeline()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            vkDestroyPipeline(_device, _raw, nullptr);
        }
    }
    GraphicsPipeline(GraphicsPipeline &&other) noexcept = default;
    GraphicsPipeline &operator=(GraphicsPipeline &&other) noexcept = default;

    static GraphicsPipeline basic(VkDevice device,
        VkPipelineLayout pipelineLayout,
        VkRenderPass renderPass,
        VkShaderModule vertexShader,
        VkShaderModule fragmentShader);
};

class DescriptorSetLayout : public DeviceConstructible<
                                VkDescriptorSetLayout,
                                VkDescriptorSetLayoutCreateInfo,
                                &vkCreateDescriptorSetLayout,
                                &vkDestroyDescriptorSetLayout> {
public:
    using DeviceConstructible::DeviceConstructible;
};

class DescriptorPool : public DeviceConstructible<
                           VkDescriptorPool,
                           VkDescriptorPoolCreateInfo,
                           &vkCreateDescriptorPool,
                           &vkDestroyDescriptorPool> {
public:
    using DeviceConstructible::DeviceConstructible;
};
}
