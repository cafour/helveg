#version 300 es

precision mediump float;

in vec4 v_color;
in vec2 v_diffVector;
in vec2 v_radii;

out vec4 f_color;

uniform float u_correctionRatio;

const vec4 transparent = vec4(0.0f, 0.0f, 0.0f, 0.0f);

void main(void) {
    float border = u_correctionRatio * 2.0f;
    float radius = v_radii.y;
    float dist = length(v_diffVector) - radius + border;

    #ifdef PICKING_MODE
    if (dist < border) {
        f_color = v_color;
    } else {
        discard;
    }
    return;
    #endif

    float t = 0.0f;
    if (dist > border)
        t = 1.0f;
    else if (dist > 0.0f)
        t = dist / border;

    f_color = mix(v_color, transparent, t);
}
