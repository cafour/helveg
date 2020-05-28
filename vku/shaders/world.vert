#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform UBO {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout(location = 0) in vec3 inPosition;
layout(location = 1) in vec3 inColor;
layout(push_constant) uniform Constants {
    vec3 offset;
};

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec3 fragPosition;

void main() {
    gl_Position = ubo.proj * ubo.view * ubo.model * vec4(inPosition + offset, 1.0f);
    fragPosition = gl_Position.xyz;
    fragColor = inColor;
}
