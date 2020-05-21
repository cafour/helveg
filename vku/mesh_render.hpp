#pragma once

#include "depth_core.hpp"
#include "display_core.hpp"
#include "instance_core.hpp"
#include "render_core.hpp"
#include "swapchain_core.hpp"
#include "wrapper.hpp"

#include <glm/glm.hpp>

namespace vku {
class MeshRender {
public:
    struct Mesh {
        glm::vec3 *vertices;
        glm::vec3 *colors;
        uint32_t *indices;
        uint32_t vertexCount;
        uint32_t indexCount;
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

    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::RenderPass _renderPass;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexBufferMemory;
    vku::Buffer _indexBuffer;
    vku::DeviceMemory _indexBufferMemory;
    std::vector<vku::Buffer> _uboBuffers;
    std::vector<vku::DeviceMemory> _uboBufferMemories;
    std::vector<VkDescriptorSet> _descriptorSets;

    Mesh _mesh;
    glm::vec3 _meshMin = {};
    glm::vec3 _meshMax = {};

    void recordCommandBuffer(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame);
    vku::Framebuffer createFramebuffer(vku::SwapchainFrame &frame);
    void onResize(size_t imageCount, VkExtent2D extent);
    void onUpdate(vku::SwapchainFrame &frame);

public:
    MeshRender(int width, int height, Mesh mesh);

    vku::InstanceCore &instanceCore() { return _instanceCore; }
    vku::DisplayCore &displayCore() { return _displayCore; }
    vku::SwapchainCore &swapchainCore() { return _swapchainCore; }
    vku::RenderCore &renderCore() { return _renderCore; }
};
}
