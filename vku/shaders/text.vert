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
    mat4 viewInv;
    mat4 projInv;
    vec3 position;
}
camera;

layout(location = 0) in vec2 inPosition;
layout(location = 1) in vec2 inUV;
layout(location = 2) in vec3 inCenter;

layout(location = 0) out vec2 outUV;

void main(void)
{
    // Based on http://www.opengl-tutorial.org/intermediate-tutorials/billboards-particles/billboards/p
    vec3 camRight = vec3(camera.view[0][0], camera.view[1][0], camera.view[2][0]);
    vec3 camUp = vec3(camera.view[0][1], camera.view[1][1], camera.view[2][1]);

    vec3 billboardPosition = inCenter
        + camRight * inPosition.x
        + camUp * inPosition.y;

    outUV = inUV;
    gl_Position = camera.proj * camera.view * model * vec4(billboardPosition, 1.0f);
}
