#version 300 es

precision mediump float;

in vec4 v_color;
in vec4 v_backgroundColor;
in vec2 v_diffVector;
in vec2 v_radii;
in float v_gap;
in float v_strokedSlice;
in float v_stroke;
in float v_hatchingWidth;
flat in float v_childrenIndicator;

out vec4 f_color;

uniform float u_pixelRatio;

const vec4 transparent = vec4(0.0f, 0.0f, 0.0f, 0.0f);
const float pi = 3.14159f;
const float eps = 0.00001f;
const float strokedLightness = 1.3f;
const float solidLightness = 1.1f;

float circle(float radius, float dist) {
    return clamp(sign(radius - dist), 0.0f, 1.0f);
}

// Returns:
// * 1 if inside the circle
// * 0 if outside the circle
// * (1, 0) if on the border of the circle
// IMPORTANT: Must be used in uniform control flow.
float smoothcircle(float radius, float dist) {
    float edge = radius - dist;
    float delta = fwidth(edge);
    float t = (edge + delta) / delta;
    return clamp(t, 0.0f, 1.0f);
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
    float diffPos = fwidth(pos) * u_pixelRatio * 2.0f;
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

    // NB: inner circle which is always present
    float opacity = 0.0f;
    float innerCircleFactor = smoothcircle(v_radii.x, dist);
    f_color = v_backgroundColor;
    opacity += innerCircleFactor;

    if (v_childrenIndicator > 0.0f) {
        float radius = max(v_radii.x, v_radii.y);
        vec2 childrenIndicatorOffset = vec2(0, -v_childrenIndicator);
        float childrenIndicatorDist = length(v_diffVector - childrenIndicatorOffset);
        float childrenIndicatorFactor = smoothcircle(radius, childrenIndicatorDist) - smoothcircle(radius + v_gap, dist);
        if (childrenIndicatorFactor > 0.0f) {
            opacity += childrenIndicatorFactor;
            f_color = v_color;
        }
    }

    if (v_radii.y > 0.0f) {
        float donutFactor = smoothcircle(v_radii.y, dist) - smoothcircle(v_radii.x + v_gap, dist);
        if (donutFactor > 0.0f) {
            opacity += donutFactor;

            // NB: x and y are switched on purpose (and y flipped as well) to emulate a rotation by 90 deg clockwise
            float halfAngle = abs(atan(v_diffVector.x, -v_diffVector.y));
            // float margin = v_stroke;
            if (v_strokedSlice < eps) {
                // full solid
                f_color = v_color;
            } else if (pi - v_strokedSlice < eps) {
                // full stroked
                f_color = v_backgroundColor;
                // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, circle(v_radii.x + v_gap + margin, dist) + 1.0f - circle(v_radii.y - margin, dist)));
                // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, hatch()));
            } else {
                // margin = is the line lining the stroked sector, it should be always at least two "pixels" wide
                float strokedSector = sector(v_strokedSlice, halfAngle, dist, v_gap);
                // by inverting the angle, we switch to the complementary angle but still correctly compute the gap
                float solidSector = sector(pi - v_strokedSlice, pi - halfAngle, dist, v_gap);

                opacity *= max(strokedSector, solidSector);
                f_color = mix(transparent, v_color, solidSector) + mix(transparent, v_backgroundColor, strokedSector);
                // top and bottom line of the stroked sector
                // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (circle(v_radii.x + v_gap + margin, dist) + 1.0f - circle(v_radii.y - margin, dist))));
                // left and right line of the stroked sector
                // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (1.0f - sector(v_strokedSlice, halfAngle, dist, v_gap + 2.0f * margin))));
                // hatching
                // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * hatch()));
            }
        }
    }

    f_color = mix(transparent, f_color, opacity * 0.9f);
}
