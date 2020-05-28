#pragma once

#include "chunk_render.hpp"
#include "instance_core.hpp"
#include "display_core.hpp"
#include "swapchain_core.hpp"
#include "render_core.hpp"
#include "depth_core.hpp"
#include "transfer_core.hpp"
#include "mesh_core.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {
class WorldRender {
public:

    struct World
    {
        vku::ChunkRender::Chunk *chunks;
        glm::vec3 *positions;
        size_t count;
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

    World _world;
    glm::vec3 _boxMin = {};
    glm::vec3 _boxMax = {};

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    WorldRender(int width, int height, World chunk);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}
