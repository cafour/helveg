#version 300 es

precision mediump float;

in vec4 v_color;

out vec4 f_color;

void main(void) {
  f_color = v_color;
}
