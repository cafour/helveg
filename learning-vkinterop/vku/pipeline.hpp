#pragma once

#include <volk.h>

#include "render_pass.hpp"
#include "shader.hpp"

namespace vku {

class Pipeline {
private:
    VkPipeline _raw;
    RenderPass &_renderPass;
    Shader &_vertexShader;
    Shader &_fragmentShader;

public:
    Pipeline(RenderPass &renderPass, Shader &vertexShader, Shader &fragmentShader);
    ~Pipeline();

    operator VkPipeline() { return _raw; }

    RenderPass &renderPass() { return _renderPass; }
    Shader &vertexShader() { return _vertexShader; }
    Shader &fragmentShader() {return _fragmentShader; }
};
}
