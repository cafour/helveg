#pragma once

#include "cores.hpp"
#include "data.hpp"

#include <glm/glm.hpp>

namespace vku {
class WorldRender {
private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::DepthCore _depthCore;
    vku::TransferCore _transferCore;
    std::vector<vku::MeshCore> _meshes;

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

    vku::World _world;
    glm::vec3 _boxMin = {};
    glm::vec3 _boxMax = {};

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    WorldRender(int width, int height, World world, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}
