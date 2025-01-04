#version 300 es

precision mediump float;

in vec2 a_position;
in float a_iconSize;
in vec4 a_texture;
in float a_angle;
in float a_grayscale;

uniform float u_sizeRatio;
uniform float u_correctionRatio;
uniform mat3 u_matrix;

out vec4 v_texture;
out vec2 v_texCoord;
flat out float v_grayscale;

const float sqrt2 = sqrt(2.0f);

void main() {
    float size = a_iconSize * u_correctionRatio / u_sizeRatio * 4.0f;
    vec2 normalizedDiffVector = vec2(cos(a_angle), sin(a_angle));
    vec2 diffVector = size * normalizedDiffVector;
    vec2 position = a_position + diffVector;
    gl_Position = vec4((u_matrix * vec3(position, 1)).xy, 0, 1);

    // v_texCoord = vec2(normalizedDiffVector.x + 0.5f, -(normalizedDiffVector + 0.5f));
    v_texCoord = vec2(normalizedDiffVector.x, -normalizedDiffVector.y) * sqrt2 + 0.5f;
    // Pass the texture coordinates:
    // NOTE: multiply a_texture by a constant and you get a pattern
    v_texture = a_texture;
    v_grayscale = a_grayscale;
}
