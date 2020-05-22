#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 0) uniform UBO {
    mat4 model;
    mat4 view;
    mat4 proj;
} ubo;

layout(binding = 1) readonly buffer ColorSSBO { vec3 colors[]; };

layout(location = 0) in vec3 inPosition;
layout(push_constant) uniform Constants {
    uint size;
};

layout(location = 0) out vec3 fragColor;
layout(location = 1) out vec3 fragPosition;

void main() {
    uint x = gl_InstanceIndex / (size * size);
    uint y = gl_InstanceIndex / size % size;
    uint z = gl_InstanceIndex % size;
    gl_Position = ubo.proj * ubo.view * ubo.model * vec4(inPosition + vec3(x, y, z) * 2, 1.0f);
    fragPosition = gl_Position.xyz;
    fragColor = colors[gl_InstanceIndex];
}
