#version 300 es

precision mediump float;

in vec4 v_color;
in vec2 v_diffVector;
in vec2 v_radii;
in float v_gap;
in float v_strokedSlice;

out vec4 f_color;

uniform float u_correctionRatio;

const vec4 transparent = vec4(0.0f, 0.0f, 0.0f, 0.0f);

// Returns:
// * 1 if inside the circle
// * 0 if outside the circle
// * (1, 0) if on the border of the circle
float circle(float radius, float dist) {
    float border = u_correctionRatio * 2.0f;
    float t = 1.0f;
    float edge = dist - radius + border;
    if (edge > border)
        t = 0.0f;
    else if (edge > 0.0f)
        t = 1.0f - edge / border;
    return t;
}

float sector(float sectorHalfAngle, float halfAngle) {
    float border = u_correctionRatio * 2.0f;
    float t = 1.0f;
    float edge = halfAngle - sectorHalfAngle + border;
    if (edge > border)
        t = 0.0f;
    else if (edge > 0.0f)
        t = 1.0f - edge / border;
    return t;
}

void main(void) {
    float dist = length(v_diffVector);

    #ifdef PICKING_MODE
    // renders a simple circle into the pickingBuffer
    if (dist < v_radii.y) {
        f_color = v_color;
    } else {
        discard;
    }
    return;
    #endif

    f_color = transparent;
    f_color = f_color + mix(transparent, v_color * 0.2f, circle(v_radii.x, dist));

    float halfAngle = abs(atan(v_diffVector.x, -v_diffVector.y));
    vec4 sector_color = mix(//
    v_color * 0.9f, //
    v_color * 0.5f, //
    sector(v_strokedSlice, halfAngle));
    f_color = f_color + mix(transparent, sector_color, circle(v_radii.y, dist) - circle(v_radii.x + v_gap, dist));

    // NB: x and y are switched on purpose (and y flipped as well) to emulate a rotation by 90 deg clockwise

}
