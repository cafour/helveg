#version 300 es

precision mediump float;

in vec4 v_color;
in float v_border;
in vec4 v_texture;
in float v_iconSize;

out vec4 f_color;

uniform sampler2D u_atlas;

const float radius = 0.5;
const float sqrt2 = sqrt(2.0);

void main(void) {
  float dist = length(gl_PointCoord - vec2(0.5, 0.5));

  vec2 texCoord = (gl_PointCoord - vec2(0.5, 0.5)) * sqrt2 + vec2(0.5, 0.5);

  if (dist < radius - v_border) {
    f_color = v_color;
  }

  // NB: the 2% is a hack to avoid artifacts at the edges of icons in an atlas
  if (texCoord.x > 0.01 && texCoord.x < 0.99 && texCoord.y > 0.01 && texCoord.y < 0.99) {
    vec4 texel = texture(u_atlas, v_texture.xy + texCoord * v_texture.zw);
    f_color = vec4(mix(f_color, texel, texel.a).rgb, max(texel.a, f_color.a));
  }

}
