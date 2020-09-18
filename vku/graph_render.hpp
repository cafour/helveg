#pragma once

#include "display_core.hpp"
#include "instance_core.hpp"
#include "render_core.hpp"
#include "swapchain_core.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {
class GraphRender {
public:
    struct Graph {
        glm::vec2 *positions;
        uint32_t *weights;
        uint32_t count;
    };

private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::PipelineLayout _pipelineLayout;
    vku::RenderPass _renderPass;
    vku::GraphicsPipeline _nodePipeline;
    vku::GraphicsPipeline _edgePipeline;
    Graph _graph;
    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::Buffer _nodeBuffer;
    vku::DeviceMemory _nodeBufferMemory;
    size_t _edgeCount;
    vku::Buffer _edgeBuffer;
    vku::DeviceMemory _edgeBufferMemory;
    std::vector<VkDescriptorSet> _descriptorSets;
    float _scale;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);

public:
    GraphRender(int width, int height, Graph graph, float scale = 0.01f, bool debug = false);

    void flushPositions();
    vku::InstanceCore &instanceCore() { return _instanceCore; }
    vku::DisplayCore &displayCore() { return _displayCore; }
    vku::SwapchainCore &swapchainCore() { return _swapchainCore; }
    vku::RenderCore &renderCore() { return _renderCore; }
};
}
