#version 300 es

precision mediump float;

in vec4 v_color;
in vec2 v_diffVector;
in vec2 v_radii;
in float v_gap;
in float v_strokedSlice;
in float v_stroke;
in float v_hatchingWidth;

out vec4 f_color;

uniform float u_pixelRatio;

const vec4 transparent = vec4(0.0f, 0.0f, 0.0f, 0.0f);
const float pi = 3.14159f;
const float eps = 0.00001f;
const float strokedFactor = 0.2f;
const float solidFactor = 0.5f;


// Returns:
// * 1 if inside the circle
// * 0 if outside the circle
// * (1, 0) if on the border of the circle
float circle(float radius, float dist) {
    float distDelta = fwidth(dist);
    float t = 1.0f;
    float edge = dist - radius + distDelta;
    if (edge > distDelta)
        t = 0.0f;
    else if (edge > 0.0f)
        t = 1.0f - edge / distDelta;
    return t;
}

float sector(float sectorHalfAngle, float halfAngle, float dist, float gap) {
    float t = 1.0f;
    float halfAngleDelta = fwidth(halfAngle);
    // NB: The 4.0f below is magic. It just kinda looks good.
    float angularGap = 4.0f * gap / (2.0f * pi * dist);
    // NB: ...the 2.0f below is also magic.
    angularGap = max(angularGap, halfAngleDelta / 2.0f);
    float edge = halfAngle - sectorHalfAngle + halfAngleDelta + angularGap;
    if (edge > halfAngleDelta)
        t = 0.0f;
    else if (edge > 0.0f)
        t = 1.0f - edge / halfAngle;
    return t;
}

float hatch() {
    float pos = v_diffVector.x + v_diffVector.y;
    float diffPos = fwidth(pos) * u_pixelRatio * 2.0;
    float pattern = fract(pos / max(2.0f * diffPos, v_hatchingWidth));
    return pattern < 0.5f ? 1.0f : 0.0f;
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

    // NB: x and y are switched on purpose (and y flipped as well) to emulate a rotation by 90 deg clockwise
    float halfAngle = abs(atan(v_diffVector.x, -v_diffVector.y));

    float margin = v_stroke;
    float sectorFactor = 0.0f;
    if (v_strokedSlice < eps) {
        // full solid
        sectorFactor = solidFactor;
    } else if (pi - v_strokedSlice < eps) {
        // full stroked
        sectorFactor = strokedFactor;
        // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, circle(v_radii.x + v_gap + margin, dist) + 1.0f - circle(v_radii.y - margin, dist)));
        sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, hatch()));
    } else {
        // margin = is the line lining the stroked sector, it should be always at least two "pixels" wide
        float strokedSector = sector(v_strokedSlice, halfAngle, dist, v_gap);
        // by inverting the angle, we switch to the complementary angle but still correctly compute the gap
        float solidSector = sector(pi - v_strokedSlice, pi - halfAngle, dist, v_gap);

        sectorFactor = mix(0.0f, strokedFactor, strokedSector);
        // top and bottom line of the stroked sector
        // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (circle(v_radii.x + v_gap + margin, dist) + 1.0f - circle(v_radii.y - margin, dist))));
        // left and right line of the stroked sector
        // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (1.0f - sector(v_strokedSlice, halfAngle, dist, v_gap + 2.0f * margin))));
        // hatching
        sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * hatch()));

        sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, solidSector));
    }

    if (v_radii.y > 0.0f) {
        f_color = f_color + mix(transparent, v_color * sectorFactor, circle(v_radii.y, dist) - circle(v_radii.x + v_gap, dist));
    }

}
