#version 300 es

precision mediump float;

in float v_coreRadius;
in float v_sauceEdge;
in float v_crustEdge;
in float v_totalSize;


uniform float u_crustWidth;
uniform float u_sauceWidth;

out vec4 f_color;

void main(void) { 
    vec2 p = (gl_PointCoord - vec2(0.5, 0.5)) * v_totalSize;
    float dist = length(p);

    if (dist < v_sauceEdge) {
        f_color = vec4(1.0, 0.0, 0.0, 1.0);
    } else if (dist < v_crustEdge) {
        f_color = vec4(1.0, 1.0, 0.0, 1.0);
    } else {
        discard;
    }
}
