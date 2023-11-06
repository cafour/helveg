#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;
uniform float u_sauceWidth;

out vec2 v_offset;
out float v_coreRadius;
out float v_sauceRadius;
out float v_totalSize;
flat out int v_index;

void main() {
    gl_Position = vec4(
        (u_matrix * vec3(a_position, 1)).xy,
        0,
        1
    );

    // HACK: This should never happen. But it does. Sometimes.
    if (a_size == 0.0) {
        return;
    }

    v_totalSize = a_size + 2.0 * u_sauceWidth * 1.05;
    gl_PointSize = v_totalSize / u_sizeRatio * u_pixelRatio * 2.0;

    v_coreRadius = a_size / 2.0;
    v_sauceRadius = v_coreRadius + 1.05 * u_sauceWidth;
    v_index = gl_VertexID;
}
