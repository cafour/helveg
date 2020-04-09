#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec3 fragPosition;

layout(location = 0) out vec4 outColor;

const vec3 lightDirection = vec3(2.0f, 2.0f, 2.0f);

void main() {
    vec3 dx = dFdx(fragPosition);
    vec3 dy = dFdy(fragPosition);
    vec3 normal = normalize(cross(dx, dy));
    float light = max(0.0f, dot(lightDirection, normal));
    outColor = light * vec4(fragColor, 1.0f);
}
