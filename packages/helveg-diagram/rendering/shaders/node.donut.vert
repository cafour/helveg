#version 300 es

precision mediump float;

in vec4 a_id;
in vec2 a_position;
in float a_baseSize;
in vec3 a_slices;
in vec4 a_color;
in float a_angle;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform float u_correctionRatio;
uniform mat3 u_matrix;
uniform float u_gap;

out vec4 v_color;
out vec2 v_diffVector;
out vec2 v_radii;

const float bias = 255.0 / 254.0;

void main() {
    float innerRadius = (a_baseSize / 2.0) * u_correctionRatio / u_sizeRatio * 4.0;
    float outerRadius = (a_baseSize / 2.0 + a_slices.z) * u_correctionRatio / u_sizeRatio * 4.0;
    vec2 diffVector = outerRadius * 2.0 * vec2(cos(a_angle), sin(a_angle));
    vec2 position = a_position + diffVector;
    gl_Position = vec4(
        (u_matrix * vec3(position, 1)).xy,
        0,
        1
    );

    v_diffVector = diffVector;
    v_radii = vec2(innerRadius, outerRadius);

    #ifdef PICKING_MODE
    v_color = a_id;
    #else
    v_color = a_color;
    v_color.a *= bias;
    #endif
}
