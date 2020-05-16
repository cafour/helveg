#version 450

layout(points) in;
layout(triangle_strip, max_vertices = 3) out;

void main()
{
    gl_Position = gl_in[0].gl_Position + vec4(-0.866f, -0.5f, 0.0f, 0.0f);
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(0.0f, 1.0f, 0.0f, 0.0f);
    EmitVertex();

    gl_Position = gl_in[0].gl_Position + vec4(0.866f, -0.5f, 0.0f, 0.0f);
    EmitVertex();

    EndPrimitive();
}
