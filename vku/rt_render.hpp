#pragma once

#include "cores.hpp"

namespace vku {
/**
 * A version of WorldRender that uses the VK_KHR_ray_tracing extension.
 **/
class RTRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;

    VkPhysicalDeviceRayTracingPropertiesKHR _rtProperties;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);

public:
    RTRender(int width, int height, World world, const std::string &title, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}