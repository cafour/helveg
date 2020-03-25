#pragma once

#include "shaders.hpp"
#include "vku.hpp"

#include <algorithm>
#include <functional>
#include <iostream>
#include <stdexcept>
#include <string>
#include <vector>

class Triangle : public vku::App {
private:
    vku::PipelineLayout _pipelineLayout;
    vku::GraphicsPipeline _pipeline;

public:

    Triangle(int width, int height)
        : vku::App("Hello, Triangle!", width, height)
    {
        _pipelineLayout = vku::PipelineLayout::basic(device());
        _pipeline = vku::GraphicsPipeline::basic(
            device(), 
            _pipelineLayout, 
            renderPass(),
            vku::ShaderModule::inlined(device(), VERTEX_SHADER, VERTEX_SHADER_LENGTH),
            vku::ShaderModule::inlined(device(), FRAGMENT_SHADER, FRAGMENT_SHADER_LENGTH));
    }
    
    void recordCommands(VkCommandBuffer commandBuffer, vku::SwapchainFrame &frame) override;
};
