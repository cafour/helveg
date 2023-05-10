#version 450

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

layout(location = 0) in vec4[] inColor;

layout(location = 0) out vec4 outColor;

layout(push_constant) uniform Constants
{
    layout(offset = 80) float aspect;
};

void main()
{
    const vec2 coordinates[] = vec2[](
        vec2(-1.0f, -1.0f),
        vec2(-1.0f, 1.0f),
        vec2(1.0f, -1.0f),
        vec2(1.0f, 1.0f));

    for (int i = 0; i < 4; i++) {
        outColor = inColor[0];
        gl_Position = gl_in[0].gl_Position + vec4(coordinates[i].x, coordinates[i].y * aspect, 0.0f, 0.0f);
        EmitVertex();
    }

    EndPrimitive();
}
