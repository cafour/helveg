#pragma once

#include "cores.hpp"
#include "data.hpp"

#include <glm/glm.hpp>

namespace vku {
class ChunkRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::DepthCore _depthCore;
    vku::TransferCore _transferCore;
    vku::MeshCore _meshCore;

    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::RenderPass _renderPass;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _colorBuffer;
    vku::DeviceMemory _colorMemory;
    std::vector<vku::Buffer> _uboBuffers;
    std::vector<vku::DeviceMemory> _uboBufferMemories;
    std::vector<VkDescriptorSet> _descriptorSets;

    vku::Chunk _chunk;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    ChunkRender(int width, int height, Chunk chunk, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}
