#version 450
#extension GL_ARB_separate_shader_objects : enable

layout(binding = 1) uniform Camera
{
    mat4 view;
    mat4 proj;
    vec3 position;
}
camera;

layout(location = 0) in vec3 fragColor;
layout(location = 1) in vec3 fragPosition;

layout(location = 0) out vec4 outColor;

struct Light {
    vec4 position;
    vec4 ambientColor;
    vec4 diffuseColor;
    vec4 specularColor;
};

layout(push_constant) uniform Constants
{
    layout(offset = 16) Light light;
};

void main()
{
    vec3 dx = dFdx(fragPosition);
    vec3 dy = dFdy(fragPosition);
    vec3 N = normalize(cross(dx, dy));
    // float light = max(0.0f, dot(lightDirection, N));
    // outColor = light * vec4(fragColor, 1.0f);

    vec3 lightVector = light.position.xyz - fragPosition * light.position.w;
    vec3 L = normalize(lightVector);
    vec3 E = normalize(camera.position - fragPosition);
    vec3 H = normalize(L + E);

    float NdotL = max(dot(N, L), 0.0f);
    float NdotH = max(dot(N, H), 0.0f);

    float distance2 = light.position.w == 1.0f ? pow(length(lightVector), 2) : 1.0f;
    float spec = pow(NdotH, 1.0f);

    vec3 color = fragColor * light.ambientColor.rgb
        + NdotL * fragColor * light.diffuseColor.rgb
        + spec * fragColor * light.specularColor.rgb;
    outColor = vec4(color / distance2, 1.0f);
}
