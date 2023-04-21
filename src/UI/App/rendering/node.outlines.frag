#version 300 es

precision mediump float;

in vec4 v_color;
in vec4 v_outlineStarts;
in vec4 v_outlineEnds;
in vec4 v_outlineStyles;

out vec4 f_color;

float outline(float width, float size, float dist, vec4 color) {
  bool isDashed = false;

  if (dist > size - width && dist < size) {
    f_color = color;
    return -1.0;
  }

  return size - width;
}

void main(void) {
  float dist = length(gl_PointCoord - vec2(0.5, 0.5)) * 2.0;

  if (dist > v_outlineStarts.x && dist < v_outlineEnds.x) {
    f_color = vec4(v_color.rgb, 0.10);
  } else if (dist > v_outlineStarts.y && dist < v_outlineEnds.y) {
    f_color = vec4(v_color.rgb, 0.60);
  } else if (dist > v_outlineStarts.z && dist < v_outlineEnds.z) {
    f_color = vec4(v_color.rgb, 0.40);
  } else if (dist > v_outlineStarts.w && dist < v_outlineEnds.w) {
    f_color = vec4(v_color.rgb, 0.20);
  }
}
