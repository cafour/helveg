#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform Model
{
    mat4 model;
};

layout(binding = 1) uniform Camera
{
    mat4 view;
    mat4 proj;
    mat4 viewInv;
    mat4 projInv;
    vec3 position;
}
camera;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec4 inColor;

layout(location = 0) out vec4 outColor;

void main()
{
    outColor = inColor;
    gl_Position = camera.proj * camera.view * model * vec4(inPosition, 1.0f);
}
