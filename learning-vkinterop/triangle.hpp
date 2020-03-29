#pragma once

#include "shaders.hpp"
#include "vku.hpp"

#include <glm/glm.hpp>

#include <algorithm>
#include <functional>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>
#include <cstddef>

#include <fstream>

struct Vertex {
    glm::vec2 position;
    glm::vec3 color;
};

struct UBO {
    glm::mat4 model;
    glm::mat4 view;
    glm::mat4 projection;
};

class Triangle : public vku::App {
private:
    vku::DescriptorSetLayout _setLayout;
    vku::DescriptorPool _descriptorPool;
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexBufferMemory;
    std::vector<vku::Buffer> _uboBuffers;
    std::vector<vku::DeviceMemory> _uboBufferMemories;
    std::vector<VkDescriptorSet> _descriptorSets;

    const std::vector<Vertex> vertices = {
        { { 0.0f, -0.5f }, { 1.0f, 0.0f, 0.0f } },
        { { 0.5f, 0.5f }, { 0.0f, 1.0f, 0.0f } },
        { { -0.5f, 0.5f }, { 0.0f, 0.0f, 1.0f } }
    };

public:
    Triangle(int width, int height);
    void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) override;
    void prepare() override;
    void update(vku::SwapchainFrame &frame) override;
};
