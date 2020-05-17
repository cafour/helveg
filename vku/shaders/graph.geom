#version 450

layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

const float scale = 0.01f;

void main()
{
    const vec2 coordinates[] = vec2[](
        vec2(-1.0f, -1.0f),
        vec2(-1.0f, 1.0f),
        vec2(1.0f, -1.0f),
        vec2(1.0f, 1.0f));

    for (int i = 0; i < 4; i++) {
        gl_Position = gl_in[0].gl_Position + vec4(coordinates[i], 0.0f, 0.0f) * scale;
        EmitVertex();
    }

    EndPrimitive();
}
