#include "rt_render.hpp"

#include <vector>

/*
 * Based on the VK_KHR_ray_tracing tutorial by NVIDIA:
 * https://nvpro-samples.github.io/vk_raytracing_tutorial_KHR/
 */

const std::vector<const char*> requiredDevicesExtensions = std::vector<const char*>{
    VK_KHR_SWAPCHAIN_EXTENSION_NAME,
    VK_KHR_RAY_TRACING_EXTENSION_NAME,
    VK_KHR_MAINTENANCE3_EXTENSION_NAME,
    VK_KHR_PIPELINE_LIBRARY_EXTENSION_NAME,
    VK_KHR_DEFERRED_HOST_OPERATIONS_EXTENSION_NAME,
    VK_KHR_BUFFER_DEVICE_ADDRESS_EXTENSION_NAME
};

const VkPhysicalDeviceRayTracingFeaturesKHR next = {
    VK_STRUCTURE_TYPE_PHYSICAL_DEVICE_RAY_TRACING_FEATURES_KHR
};

vku::RTRender::RTRender(int width, int height, World world, const std::string &title, bool debug)
    : _instanceCore("RTRender", true, true)
    , _displayCore(_instanceCore.instance(), width, height, "RTRender", requiredDevicesExtensions, nullptr, &next)
    , _swapchainCore(_displayCore)
    , _renderCore(
        _displayCore,
        _swapchainCore,
        [this](auto &f) { return createFramebuffer(f); },
        [this](auto cb, auto &f) { recordCommandBuffer(cb, f); })
{
}

void vku::RTRender::recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame)
{
}

vku::Framebuffer vku::RTRender::createFramebuffer(vku::SwapchainFrame &frame)
{
    return {};
}