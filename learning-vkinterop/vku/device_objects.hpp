#pragma once

#include "base.hpp"

#include <volk.h>

namespace vku {

template <typename T, typename TCreateInfo>
using DeviceObjectConstructor = VkResult (**)(
    VkDevice device,
    const TCreateInfo *pCreateInfo,
    const VkAllocationCallbacks *pAllocator,
    T *pDeviceObject);

template <typename T>
using DeviceObjectDestructor = void (**)(
    VkDevice device,
    T deviceObject,
    const VkAllocationCallbacks *pAllocator);

template <
    typename T,
    typename TCreateInfo,
    DeviceObjectConstructor<T, TCreateInfo> vkConstructor,
    DeviceObjectDestructor<T> vkDestructor>
class DeviceObject {
protected:
    VkDevice _device;
    T _raw;

public:
    DeviceObject(VkDevice device, TCreateInfo &createInfo)
        : _device(device)
    {
        ENSURE((*vkConstructor)(device, &createInfo, nullptr, &_raw));
    }
    DeviceObject(VkDevice device, T raw)
        : _device(device)
        , _raw(raw)
    {
    }
    ~DeviceObject()
    {
        if (_device != VK_NULL_HANDLE && _raw != VK_NULL_HANDLE) {
            (*vkDestructor)(_device, _raw, nullptr);
        }
    }
    DeviceObject(const DeviceObject &other) = delete;
    DeviceObject(DeviceObject &&other) noexcept
        : _device(std::exchange(other._device, nullptr))
        , _raw(std::exchange(other._raw, nullptr))
    {
    }
    DeviceObject &operator=(const DeviceObject &other) = delete;
    DeviceObject &operator=(DeviceObject &&other) noexcept
    {
        if (this != &other) {
            std::swap(_device, other._device);
            std::swap(_raw, other._raw);
        }
        return *this;
    }

    operator T() { return _raw; }
    T raw() { return _raw; }
    VkDevice device() { return _device; }
};

class Semaphore : public DeviceObject<
                      VkSemaphore,
                      VkSemaphoreCreateInfo,
                      &vkCreateSemaphore,
                      &vkDestroySemaphore> {
public:
    using DeviceObject::DeviceObject;
    static Semaphore basic(VkDevice device);
};

class Fence : public DeviceObject<
                  VkFence,
                  VkFenceCreateInfo,
                  &vkCreateFence,
                  &vkDestroyFence> {
public:
    using DeviceObject::DeviceObject;
    static Fence basic(VkDevice device);
};

class ImageView : public DeviceObject<
                      VkImageView,
                      VkImageViewCreateInfo,
                      &vkCreateImageView,
                      &vkDestroyImageView> {
public:
    using DeviceObject::DeviceObject;
    static ImageView basic(VkDevice device, VkImage image, VkFormat format);
};

class RenderPass : public DeviceObject<
                       VkRenderPass,
                       VkRenderPassCreateInfo,
                       &vkCreateRenderPass,
                       &vkDestroyRenderPass> {
public:
    using DeviceObject::DeviceObject;
    static vku::RenderPass basic(VkDevice device, VkFormat colorFormat);
};

class Swapchain : public DeviceObject<
                      VkSwapchainKHR,
                      VkSwapchainCreateInfoKHR,
                      &vkCreateSwapchainKHR,
                      &vkDestroySwapchainKHR> {
public:
    using DeviceObject::DeviceObject;
    static Swapchain basic(
        VkDevice device,
        VkPhysicalDevice physicalDevice,
        VkSurfaceKHR surface,
        VkSurfaceCapabilitiesKHR *pSurfaceCapabilities = nullptr,
        VkSurfaceFormatKHR *pSurfaceFormat = nullptr,
        VkSwapchainKHR old = VK_NULL_HANDLE);
};

class CommandPool : public DeviceObject<
                        VkCommandPool,
                        VkCommandPoolCreateInfo,
                        &vkCreateCommandPool,
                        &vkDestroyCommandPool> {
public:
    using DeviceObject::DeviceObject;
    static CommandPool basic(VkDevice device, uint32_t queueIndex);
};

class PipelineLayout : public DeviceObject<
                           VkPipelineLayout,
                           VkPipelineLayoutCreateInfo,
                           &vkCreatePipelineLayout,
                           &vkDestroyPipelineLayout> {
public:
    using DeviceObject::DeviceObject;
    static PipelineLayout basic(VkDevice device);
};

class ShaderModule : public DeviceObject<
                         VkShaderModule,
                         VkShaderModuleCreateInfo,
                         &vkCreateShaderModule,
                         &vkDestroyShaderModule> {
public:
    using DeviceObject::DeviceObject;
    static ShaderModule inlined(VkDevice device, const uint32_t *code, size_t length);
    static ShaderModule fromFile(VkDevice device, const char *filename);
};

class Framebuffer : public DeviceObject<
                        VkFramebuffer,
                        VkFramebufferCreateInfo,
                        &vkCreateFramebuffer,
                        &vkDestroyFramebuffer> {
public:
    using DeviceObject::DeviceObject;
    static Framebuffer basic(
        VkDevice device,
        VkRenderPass renderPass,
        VkImageView imageView,
        uint32_t width,
        uint32_t height);
};
}
