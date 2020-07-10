#pragma once

#include "cores.hpp"
#include "data.hpp"

#include <glm/glm.hpp>

namespace vku {
class WorldRender {
private:
    // Cores
    vku::InstanceCore _instanceCore;
    vku::DisplayCore _displayCore;
    vku::SwapchainCore _swapchainCore;
    vku::RenderCore _renderCore;
    vku::CameraCore _cameraCore;
    vku::DepthCore _depthCore;
    vku::TransferCore _transferCore;
    std::vector<vku::MeshCore> _meshes;

    // Common
    vku::Time _time = {};
    vku::Buffer _timeBuffer;
    vku::DeviceMemory _timeMemory;

    // Fire Compute
    vku::PipelineLayout _firePL;
    vku::DescriptorSetLayout _fireDSL;
    vku::DescriptorPool _fireDP;
    VkDescriptorSet _fireDS;
    vku::ComputePipeline _fireCP;
    vku::Buffer _emitterBuffer;
    vku::DeviceMemory _emitterMemory;
    vku::Buffer _particleBuffer;
    vku::DeviceMemory _particleMemory;

    // World Graphics
    vku::World _world;
    glm::vec3 _boxMin = {};
    glm::vec3 _boxMax = {};
    vku::PipelineLayout _worldPL;
    vku::DescriptorSetLayout _worldDSL;
    vku::DescriptorPool _worldDP;
    std::vector<VkDescriptorSet> _worldDSs;
    vku::GraphicsPipeline _worldGP;
    vku::RenderPass _renderPass;
    vku::Buffer _colorBuffer;
    vku::DeviceMemory _colorMemory;
    std::vector<vku::Buffer> _uboBuffers;
    std::vector<vku::DeviceMemory> _uboBufferMemories;

    // Fire Graphics
    vku::GraphicsPipeline _fireGP;

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

    // I needed to split the constructor to be able to find anything in this mess.
    void createMeshes();
    void createWorldGP();
    void createFireGP();
    void createFireCP();

public:
    WorldRender(int width, int height, World world, bool debug = false);

    vku::RenderCore &renderCore() { return _renderCore; }
};
}
