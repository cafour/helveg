#pragma once

#include "instance_core.hpp"
#include "display_core.hpp"
#include "swapchain_core.hpp"

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

    Graph *_graph;

    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _positionBuffer;
    vku::DeviceMemory _positionBufferMemory;
    std::vector<VkDescriptorSet> _descriptorSets;

public:
    GraphRender(int width, int height, Graph *mesh);
    void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) override;
    void prepare() override;
    void update(vku::SwapchainFrame &frame) override;
};
