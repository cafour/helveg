#pragma once

#include <functional>
#include <optional>

#include "base.hpp"
#include "containers.hpp"
#include "device_related.hpp"
#include "instance_related.hpp"
#include "render_pass.hpp"
#include "standalone.hpp"
#include "display_core.hpp"
#include "swapchain_core.hpp"

namespace vku {
/**
 * Contains the core graphics loop.
 */
class RenderCore {
private:
    vku::DisplayCore &_displayCore;
    vku::SwapchainCore &_swapchainCore;
    vku::CommandPool _commandPool;
    std::vector<vku::Framebuffer> _framebuffers;
    vku::CommandBuffers _commandBuffers;

    std::function<vku::Framebuffer (vku::SwapchainFrame &)> _createFramebuffer;
    std::function<void (VkCommandBuffer, vku::SwapchainFrame &)> _recordCommandBuffer;
    std::vector<std::function<void (vku::SwapchainFrame &)>> _updateHandlers;
    std::vector<std::function<void (size_t imageCount, VkExtent2D extent)>> _resizeHandlers;

public:
    RenderCore(
        vku::DisplayCore &displayCore,
        vku::SwapchainCore &swapchainCore,
        std::function<vku::Framebuffer (vku::SwapchainFrame &)> createFramebuffer,
        std::function<void (VkCommandBuffer, vku::SwapchainFrame &)> recordCommandBuffer);

    vku::DisplayCore &displayCore() { return _displayCore; }
    vku::CommandPool &commandPool() { return _commandPool; }
    std::vector<vku::Framebuffer> &framebuffers() { return _framebuffers; }
    vku::CommandBuffers &commandBuffers() { return _commandBuffers; }

    void onUpdate(std::function<void (vku::SwapchainFrame &)> handler);
    void onResize(std::function<void (size_t imageCount, VkExtent2D extent)> handler);

    void run();
    void resize();
    void step();
};
}
