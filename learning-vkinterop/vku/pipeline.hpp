#pragma once

#include <volk.h>

#include "render_pass.hpp"
#include "shader.hpp"

namespace vku {

class Pipeline {
private:
    VkPipeline _raw;
    VkPipelineLayout _layout;
    RenderPass &_renderPass;

public:
    Pipeline(RenderPass &renderPass, Shader &&vertexShader, Shader &&fragmentShader);
    ~Pipeline() {
        vkDestroyPipelineLayout(_renderPass.device(), _layout, nullptr);
        vkDestroyPipeline(_renderPass.device(), _raw, nullptr);
    }

    operator VkPipeline() { return _raw; }

    RenderPass &renderPass() { return _renderPass; }
};
}
