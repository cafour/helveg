#version 450

layout(points) in;
layout(triangle_strip, max_vertices = 3) out;

const float scale = 0.01f;

void main()
{
    gl_Position = gl_in[0].gl_Position + vec4(-0.866f, -0.5f, 0.0f, 0.0f) * scale;
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(0.0f, 1.0f, 0.0f, 0.0f) * scale;
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(0.866f, -0.5f, 0.0f, 0.0f) * scale;
    EmitVertex();

    EndPrimitive();
}
