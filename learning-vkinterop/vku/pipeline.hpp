#pragma once

#include <volk.h>

#include "shader.hpp"
#include "swapchain.hpp"

namespace vku {

class Pipeline {
private:
    VkPipeline _raw;
    VkPipelineLayout _layout;
    Swapchain &_swapchain;

public:
    Pipeline(Swapchain &swapchain, Shader &&vertexShader, Shader &&fragmentShader);
    ~Pipeline()
    {
        vkDestroyPipelineLayout(_swapchain.device(), _layout, nullptr);
        vkDestroyPipeline(_swapchain.device(), _raw, nullptr);
    }

    operator VkPipeline() { return _raw; }
    VkPipeline raw() { return _raw; }

    Swapchain &renderPass() { return _swapchain; }
};
}
