precision mediump float;

// varying vec4 v_color;
varying float v_border;
varying vec4 v_texture;

uniform sampler2D u_atlas;
uniform float u_keepWithinCircle;

const float radius = 0.5;

void main(void) {
  vec4 texel = texture2D(u_atlas, v_texture.xy + gl_PointCoord * v_texture.zw, -1.0);
  // vec4 color = mix(gl_FragColor, v_color, texel.a - 1.0);
  vec4 color = texel;

  vec2 m = gl_PointCoord - vec2(0.5, 0.5);
  float dist = length(m) * u_keepWithinCircle;

  if (dist < radius - v_border) {
    gl_FragColor = color;
  }
}
