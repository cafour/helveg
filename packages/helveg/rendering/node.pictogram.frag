#version 300 es

precision mediump float;

in vec4 v_color;
in float v_border;
in vec4 v_texture;

out vec4 f_color;

uniform sampler2D u_atlas;
uniform float u_keepWithinCircle;

const float radius = 0.5;

void main(void) {
  vec4 texel = texture(u_atlas, v_texture.xy + gl_PointCoord * v_texture.zw);
  vec4 color = v_color;
  color = texel;
//   color = texel;

  vec2 m = gl_PointCoord - vec2(0.5, 0.5);
  float dist = length(m) * u_keepWithinCircle;

  if (dist < radius - v_border) {
    f_color = color;
  }
}
