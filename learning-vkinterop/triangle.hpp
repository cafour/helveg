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

class Triangle : public vku::App {
private:
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;
    vku::Buffer _vertexBuffer;
    vku::DeviceMemory _vertexBufferMemory;

    const std::vector<Vertex> vertices = {
        { { 0.0f, -0.5f }, { 1.0f, 0.0f, 0.0f } },
        { { 0.5f, 0.5f }, { 0.0f, 1.0f, 0.0f } },
        { { -0.5f, 0.5f }, { 0.0f, 0.0f, 1.0f } }
    };

public:
    Triangle(int width, int height)
        : vku::App("Hello, Triangle!", width, height)
    {
        std::fstream fuck("fuck.spv", std::ios::out | std::ios::binary);
        fuck.write(static_cast<const char *>(static_cast<const void *>(TRIANGLE_VERT)), TRIANGLE_VERT_LENGTH);
        fuck.close();

        _pipelineLayout = vku::PipelineLayout::basic(device());

        VkVertexInputBindingDescription vertexBinding{
            0,
            sizeof(Vertex),
            VK_VERTEX_INPUT_RATE_VERTEX
        };

        VkVertexInputAttributeDescription vertexAttributes[2] = {
            {
                0,
                0,
                VK_FORMAT_R32G32_SFLOAT,
                offsetof(Vertex, position)
            },
            {
                1,
                0,
                VK_FORMAT_R32G32B32_SFLOAT,
                offsetof(Vertex, color)
            }
        };

        _pipeline = vku::GraphicsPipeline::basic(
            device(),
            _pipelineLayout,
            renderPass(),
            vku::ShaderModule::inlined(device(), TRIANGLE_VERT, TRIANGLE_VERT_LENGTH),
            // vku::ShaderModule::fromFile(device(), "generated/triangle.vert.spv"),
            vku::ShaderModule::inlined(device(), TRIANGLE_FRAG, TRIANGLE_FRAG_LENGTH),
            &vertexBinding,
            1,
            vertexAttributes,
            2);

        _vertexBuffer = vku::Buffer::exclusive(
            device(),
            vertices.size() * sizeof(Vertex),
            VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
        
        _vertexBufferMemory = vku::DeviceMemory::deviceLocalData(
            physicalDevice(),
            device(),
            commandPool(),
            queue(),
            _vertexBuffer,
            vertices.data(),
            vertices.size() * sizeof(Vertex));
    }

    void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) override;
};
