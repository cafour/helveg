#version 300 es

precision mediump float;

in vec4 v_diffColor;

out vec4 f_color;

const float radius = 0.5;

void main(void) {
  float dist = length(gl_PointCoord - vec2(0.5, 0.5));

  if (dist > radius) {
    discard;
  }

  f_color = v_diffColor;
}
