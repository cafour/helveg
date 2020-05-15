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
    VkRenderPass _renderPass;
    vku::CommandPool _commandPool;
    std::vector<vku::Framebuffer> _framebuffers;
    vku::CommandBuffers _commandBuffers;

    std::function<vku::Framebuffer (vku::SwapchainFrame &)> _createFramebuffer;
    std::function<void (VkCommandBuffer, vku::SwapchainFrame &)> _recordCommandBuffer;
    std::optional<std::function<void (vku::SwapchainFrame &)>> _onUpdate;
    std::optional<std::function<void (size_t, VkExtent2D)>> _onResize;

    void resize();
    void step();
    void update(vku::SwapchainFrame &frame);

public:
    RenderCore(
        vku::DisplayCore &displayCore,
        vku::SwapchainCore &swapchainCore,
        VkRenderPass renderPass,
        std::function<vku::Framebuffer (vku::SwapchainFrame &)> createFramebuffer,
        std::function<void (VkCommandBuffer, vku::SwapchainFrame &)> recordCommandBuffer,
        std::optional<std::function<void (vku::SwapchainFrame &)>> onUpdate = std::nullopt,
        std::optional<std::function<void (size_t, VkExtent2D)>> onResize = std::nullopt);

    vku::CommandPool &commandPool() { return _commandPool; }
    std::vector<vku::Framebuffer> &framebuffers() { return _framebuffers; }
    vku::CommandBuffers &commandBuffers() { return _commandBuffers; }

    void run();
};
}
