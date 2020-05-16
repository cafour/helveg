#pragma once

#include "display_core.hpp"
#include "instance_core.hpp"
#include "render_core.hpp"
#include "swapchain_core.hpp"
#include "vku.hpp"

#include <glm/glm.hpp>

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
    vku::GraphicsPipeline _pipeline;
    Graph *_graph;
    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::Buffer _positionBuffer;
    vku::DeviceMemory _positionBufferMemory;
    std::vector<VkDescriptorSet> _descriptorSets;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);

public:
    GraphRender(int width, int height, Graph *graph);

    void run() { _renderCore.run(); }
};
