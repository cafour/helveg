#version 300 es

precision mediump float;

in vec4 v_color;
in vec4 v_outlineStarts;
in vec4 v_outlineEnds;
in vec4 v_outlineStyles;

out vec4 f_color;

const float PI = 3.1415926538;
const float dashingAngle = 0.5235987756;

float outline(float width, float size, float dist, vec4 color) {
  bool isDashed = false;

  if (dist > size - width && dist < size) {
    f_color = color;
    return -1.0;
  }

  return size - width;
}

void main(void) {
  vec2 m = gl_PointCoord - vec2(0.5, 0.5);
  float dist = length(m) * 2.0;

  #ifdef PICKING_MODE
  if (dist < v_outlineEnds.w) {
    f_color = v_color;
  } else {
    discard;
  }
  return;
  #endif

  float a = f_color.a;

  float style = 0.0;

  if (dist > v_outlineStarts.x && dist < v_outlineEnds.x) {
    style = v_outlineStyles.x;
    f_color = v_color * 0.1;
  } else if (dist > v_outlineStarts.y && dist < v_outlineEnds.y) {
    style = v_outlineStyles.y;
    f_color = v_color * 0.6;
  } else if (dist > v_outlineStarts.z && dist < v_outlineEnds.z) {
    style = v_outlineStyles.z;
    f_color = v_color * 0.4;;
  } else if (dist > v_outlineStarts.w && dist < v_outlineEnds.w) {
    style = v_outlineStyles.w;
    f_color = v_color * 0.2;
  }

  if (style > 0.0) {
    float angle = atan(m.y, m.x) + PI + dashingAngle * 0.5;
    if (mod(angle / dashingAngle, 2.0) > 1.0 ) {
      discard;
    }
  }

  // f_color.a = max(a, f_color.a);
}
