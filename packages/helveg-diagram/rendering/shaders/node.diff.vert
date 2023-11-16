#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;
in vec4 a_diffColor;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;

out vec4 v_diffColor;

void main() {
  gl_Position = vec4(
    (u_matrix * vec3(a_position, 1)).xy,
    0,
    1
  );

  v_diffColor = a_diffColor;

  // Multiply the point size twice:
  //  - x SCALING_RATIO to correct the canvas scaling
  //  - x 2 to correct the formulae
  gl_PointSize = a_size / u_sizeRatio * u_pixelRatio * 2.0;
}
