#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;
uniform float u_crustWidth;
uniform float u_sauceWidth;

out float v_coreRadius;
out float v_crustRadius;
out float v_sauceRadius;
out float v_totalSize;
flat out int v_index;

void main() {
    gl_Position = vec4(
        (u_matrix * vec3(a_position, 1)).xy,
        0,
        1
    );

    // Multiply the point size twice:
    //  - x SCALING_RATIO to correct the canvas scaling
    //  - x 2 to correct the formulae
    v_totalSize = a_size + 2.0 * u_crustWidth + 2.0 * u_sauceWidth;
    gl_PointSize = v_totalSize / u_sizeRatio * u_pixelRatio * 2.0;

    v_coreRadius = a_size / 2.0;
    v_sauceRadius = v_coreRadius + u_sauceWidth;
    v_crustRadius = v_coreRadius + u_sauceWidth + 0.95 * u_crustWidth;
    v_index = gl_VertexID;
}
