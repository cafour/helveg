#version 450 core

// Based on https://github.com/SaschaWillems/Vulkan/blob/master/data/shaders/glsl/base/textoverlay.vert
// by Sascha Willems, licensed under the MIT license

layout(binding = 0) uniform Camera
{
    mat4 view;
    mat4 proj;
    vec3 position;
}
camera;

layout(location = 0) in vec2 inPos;
layout(location = 1) in vec2 inUV;

layout(location = 0) out vec2 outUV;

out gl_PerVertex
{
    vec4 gl_Position;
};

void main(void)
{
    gl_Position = vec4(inPos, 0.0, 1.0);
    outUV = inUV;
}
