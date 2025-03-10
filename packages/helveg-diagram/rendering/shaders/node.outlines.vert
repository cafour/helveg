#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;
in vec4 a_color;
in vec4 a_outlineWidths;
in vec4 a_outlineStyles;
in float a_collapsed;
in vec4 a_id;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;
uniform float u_gap;

out vec4 v_color;
out vec4 v_outlineStarts;
out vec4 v_outlineEnds;
out vec4 v_outlineStyles;

const float bias = 255.0f / 254.0f;
const vec4 WHITE = vec4(1.0f, 1.0f, 1.0f, 1.0f);

void main() {
  gl_Position = vec4((u_matrix * vec3(a_position, 1)).xy, 0, 1);

  // Multiply the point size twice:
  //  - x SCALING_RATIO to correct the canvas scaling
  //  - x 2 to correct the formulae
  gl_PointSize = a_size / u_sizeRatio * u_pixelRatio * 2.0f;

  #ifdef PICKING_MODE

  v_color = a_id;

  #else

  // Extract the color:
  v_color = a_color;
  v_color.a *= bias;

  if (a_collapsed > 0.0f) {
    v_color = mix(WHITE, v_color, 0.6f);
  }

  #endif

  float gap = u_gap / a_size;
  v_outlineStarts = vec4(0.0f, a_outlineWidths.x, a_outlineWidths.x + a_outlineWidths.y, a_outlineWidths.x + a_outlineWidths.y + a_outlineWidths.z);

  vec3 gaps = gap * sign(vec3(a_outlineWidths.y + a_outlineWidths.z + a_outlineWidths.w, a_outlineWidths.z + a_outlineWidths.w, a_outlineWidths.w));

  v_outlineEnds = vec4(v_outlineStarts.yzw - gaps, 1.0f);
  v_outlineStyles = a_outlineStyles;
}
