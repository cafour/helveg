#pragma once

#include "display_core.hpp"
#include "instance_core.hpp"
#include "swapchain_core.hpp"
#include "render_core.hpp"
#include "wrapper.hpp"

namespace vku {

class TriangleRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::RenderPass _renderPass;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);

public:
    TriangleRender(int width, int height);

    vku::InstanceCore &instanceCore() { return _instanceCore; }
    vku::DisplayCore &displayCore() { return _displayCore; }
    vku::SwapchainCore &swapchainCore() { return _swapchainCore; }
    vku::RenderCore &renderCore() { return _renderCore; }
};
}
