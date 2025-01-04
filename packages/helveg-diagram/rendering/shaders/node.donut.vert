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
out float v_strokedSlice;
out float v_gap;

const float pi = 3.14159f;
const float bias = 255.0f / 254.0f;

void main() {
    // I'm not sure why I have to multiply radii by 2.0, but Sigma's node-circle does it as well to get the same
    // sizes as in node-point. Maybe it's because of multisampling but who knows.
    float innerRadius = a_baseSize * u_correctionRatio / u_sizeRatio * 2.0;
    float outerRadius = innerRadius;
    v_gap = 0.0f;
    if (a_slices.z > 0.0f) {
        v_gap = u_gap * u_correctionRatio / u_sizeRatio;
        outerRadius = (a_baseSize + a_slices.z + v_gap) * u_correctionRatio / u_sizeRatio * 2.0;
    }

    // x2 because the triangle reaches only -0.5 on the left side and we want the triangle to contain the unit circle
    vec2 diffVector = outerRadius * 2.0f * vec2(cos(a_angle), sin(a_angle));
    vec2 position = a_position + diffVector;
    gl_Position = vec4((u_matrix * vec3(position, 1)).xy, 0, 1);

    v_diffVector = diffVector;
    v_radii = vec2(innerRadius, outerRadius);
    v_strokedSlice = a_slices.x / (a_slices.x + a_slices.y) * pi;

    #ifdef PICKING_MODE
    v_color = a_id;
    #else
    v_color = a_color;
    v_color.a *= bias;
    #endif
}
