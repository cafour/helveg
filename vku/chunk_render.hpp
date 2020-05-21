#pragma once

#include "instance_core.hpp"
#include "display_core.hpp"
#include "swapchain_core.hpp"
#include "render_core.hpp"
#include "depth_core.hpp"
#include "transfer_core.hpp"
#include "inline_mesh_core.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {
class ChunkRender {
public:
    struct Chunk
    {
        glm::vec3 *voxels;
        uint32_t size;
    };

    struct UBO {
        alignas(16) glm::mat4 model;
        alignas(16) glm::mat4 view;
        alignas(16) glm::mat4 projection;
    };

private:
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::DepthCore _depthCore;
    vku::TransferCore _transferCore;
    vku::InlineMeshCore _cubeCore;

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

    Chunk _chunk;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    ChunkRender(int width, int height, Chunk chunk);
};
}
