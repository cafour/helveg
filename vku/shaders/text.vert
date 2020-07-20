#version 450 core

// Based on https://github.com/SaschaWillems/Vulkan/blob/master/data/shaders/glsl/base/textoverlay.vert
// by Sascha Willems, licensed under the MIT license

layout(binding = 0) uniform Model
{
    mat4 model;
};
layout(binding = 1) uniform Camera
{
    mat4 view;
    mat4 proj;
    vec3 position;
}
camera;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec2 inUV;

layout(location = 0) out vec2 outUV;

void main(void)
{
    outUV = inUV;
    gl_Position = camera.proj * camera.view * model * vec4(inPosition, 1.0f);
}
