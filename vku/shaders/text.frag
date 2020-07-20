#version 450 core

// Based on https://github.com/SaschaWillems/Vulkan/blob/master/data/shaders/glsl/base/textoverlay.vert
// by Sascha Willems, licensed under the MIT license

layout(location = 0) in vec2 inUV;

layout(binding = 2) uniform sampler2D samplerFont;

layout(location = 0) out vec4 outFragColor;

void main(void)
{
    float color = texture(samplerFont, inUV).r;
    outFragColor = vec4(vec3(color), 1.0);
}
