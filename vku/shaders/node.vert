#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec2 inPosition;

layout(push_constant) uniform Constants
{
    layout(offset = 0) float scale;
};

void main() {
    gl_Position = vec4(inPosition * scale, 0.0f, 1.0f);
}
