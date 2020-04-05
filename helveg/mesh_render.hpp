#pragma once

#include "vku.hpp"

#include <glm/glm.hpp>

class MeshRender : public vku::App {
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
    Mesh _mesh;
    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexBufferMemory;
    vku::Buffer _indexBuffer;
    vku::DeviceMemory _indexBufferMemory;
    std::vector<vku::Buffer> _uboBuffers;
    std::vector<vku::DeviceMemory> _uboBufferMemories;
    std::vector<VkDescriptorSet> _descriptorSets;

public:
    MeshRender(int width, int height, Mesh mesh);
    void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) override;
    void prepare() override;
    void update(vku::SwapchainFrame &frame) override;
};
