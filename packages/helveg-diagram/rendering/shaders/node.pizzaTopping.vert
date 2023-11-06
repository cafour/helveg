#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;
in vec4 a_texture;
in vec4 a_outlines;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;

out vec4 v_texture;
out float v_size;
out vec2 v_rotation;

const float bias = 255.0 / 254.0;

void main() {
  gl_Position = vec4(
    (u_matrix * vec3(a_position, 1)).xy,
    0,
    1
  );

  // Multiply the point size twice:
  //  - x SCALING_RATIO to correct the canvas scaling
  //  - x 2 to correct the formulae
  gl_PointSize = a_size / u_sizeRatio * u_pixelRatio * 2.0;

  // Pass the texture coordinates:
  // NOTE: multiply a_texture by a constant and you get a pattern
  v_texture = a_texture;

  v_size = a_size;
  v_rotation = vec2((gl_VertexID / 2) % 2 == 0 ? -1.0 : 1.0, gl_VertexID % 2 == 0 ? 1.0 : -1.0 );
}
