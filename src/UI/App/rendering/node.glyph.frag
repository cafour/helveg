#version 300 es

precision mediump float;

in vec4 v_color;
in float v_border;
in vec4 v_texture;
flat in vec4 v_outlines;
in float v_size;

out vec4 f_color;

uniform sampler2D u_atlas;

const float radius = 0.5;
const float isqrt2 = 1.0 / sqrt(2.0);

float outline(float width, float size, float dist, vec4 color) {
  bool isDashed = false;
  // if (raw > 128.0) {
  //   // dashed
  //   isDashed = true;
  //   raw -= 128.0;
  // }

  if (dist > size - width && dist < size) {
    f_color = color;
    return -1.0;
  }

  return size - width;
}

void main(void) {
  float dist = length(gl_PointCoord - vec2(0.5, 0.5)) * v_size * 2.0;

  float currentSize = v_size;
  if (v_outlines.z > 0.0) {
    currentSize = outline(v_outlines.z, currentSize, dist, vec4(v_color.rgb, v_color.a * 0.5)) - 1.0;
    if (currentSize < 0.0) {
      // it's the outermost outline
      return;
    }
  }

  if (v_outlines.y > 0.0) {
    currentSize = outline(v_outlines.y, currentSize, dist, vec4(v_color.rgb, v_color.a * 0.75)) - 1.0;
    if(currentSize < 0.0) {
      // it's the middle outline
      return;
    }
  }

  if (v_outlines.x > 0.0) {
    currentSize = outline(v_outlines.x, currentSize, dist, vec4(v_color.rgb, v_color.a * 1.0));
    if (currentSize < 0.0) {
      // it's the innermost outline
      return;
    }
  }

  if (dist > currentSize) {
    return;
  }

  // it's the icon
  currentSize *= isqrt2;
  vec2 texCoord = (gl_PointCoord - vec2(0.5, 0.5)) * (v_size / currentSize) + vec2(0.5, 0.5);
  if (texCoord.x < 0.0 || texCoord.x > 1.0 || texCoord.y < 0.0 || texCoord.y > 1.0) {
    return;
  }
  f_color = texture(u_atlas, v_texture.xy + texCoord * v_texture.zw);

  // vec2 m = gl_PointCoord - vec2(0.5, 0.5);
  // float dist = length(m) * u_keepWithinCircle;

  // if (dist < radius - v_border) {
  //   f_color = color;
  // }
  
}
