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
layout(location = 1) in vec3 inColor;

layout(push_constant) uniform Constants
{
    layout(offset = 0) vec3 offset;
};

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec3 fragPosition;

void main()
{
    vec4 position = model * vec4(inPosition + offset, 1.0f);
    fragPosition = position.rgb / position.w;
    gl_Position = camera.proj * camera.view * position;
    fragColor = inColor;
}
