#version 300 es

precision mediump float;

in vec4 v_color;
in vec4 v_backgroundColor;
in vec2 v_diffVector;
in vec2 v_radii;
in float v_gap;
in float v_bottomSlice;
flat in float v_childrenIndicator;
flat in float v_contour;

out vec4 f_color;

uniform float u_pixelRatio;
uniform float u_hatchingWidth;

const vec4 TRANSPARENT = vec4(0.0f, 0.0f, 0.0f, 0.0f);
const vec4 WHITE = vec4(1.0f, 1.0f, 1.0f, 1.0f);
const float PI = 3.14159f;
const float TWO_PI = 6.28319f;
const float EPS = 0.00001f;

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
    float angularGap = 4.0f * gap / (2.0f * PI * dist);
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
    // float diffPos = fwidth(pos) * u_pixelRatio * 2.0f;
    // float pattern = fract(pos / max(2.0f * diffPos, u_hatchingWidth));
    float pattern = fract(pos / u_hatchingWidth);
    return pattern < 0.333f ? 1.0f : 0.0f;
}

float octagonContour() {
    // polar coordinates
    float angle = atan(v_diffVector.y, v_diffVector.x) + PI / 8.0f;
    float radius = TWO_PI / float(8);

    float field = cos(floor(0.5f + angle / radius) * radius - angle) * length(v_diffVector);
    field /= v_radii.x;

    float outline = (field < 0.9f ? 1.0f : 0.0f) - (field < 0.8f ? 1.0f : 0.0f);
    return outline;
}

float hexagonContour() {
    // polar coordinates
    float angle = atan(v_diffVector.y, v_diffVector.x) + PI / 6.0f;
    float radius = TWO_PI / float(6);

    float stripes = cos(floor(0.5f + angle / radius) * radius - angle);
    float field = stripes * length(v_diffVector);
    field /= v_radii.x;
    stripes = stripes < 0.98f ? 1.0f : 0.0f;

    float outline = (field < 0.85f ? 1.0f : 0.0f) - (field < 0.75f ? 1.0f : 0.0f);
    return outline * stripes;
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
    float lightness = 0.9f;
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
            if (v_bottomSlice < EPS) {
                // only the "top" slice
                f_color = v_color;
            } else if (PI - v_bottomSlice < EPS) {
                // only the "bottom slice
                f_color = v_backgroundColor;
                if (u_hatchingWidth > 0.0f) {
                    f_color = mix(v_backgroundColor, v_color, hatch());
                }
                lightness *= 1.33f;
            } else {
                float bottomSector = sector(v_bottomSlice, halfAngle, dist, v_gap);
                // by inverting the angle, we switch to the complementary angle but still correctly compute the gap
                float topSector = sector(PI - v_bottomSlice, PI - halfAngle, dist, v_gap);

                opacity *= max(bottomSector, topSector);
                lightness *= max(bottomSector * 1.33f, topSector);

                vec4 bottomSectorColor = v_backgroundColor;
                if (u_hatchingWidth > 0.0f) {
                    bottomSectorColor = mix(v_backgroundColor, v_color, hatch());
                }

                f_color = mix(TRANSPARENT, v_color, topSector) + mix(TRANSPARENT, bottomSectorColor, bottomSector);
            }
        }
    }

    if (v_contour == 1.0f) {
        f_color = mix(f_color, v_color, octagonContour());
    } else if (v_contour == 2.0f) {
        f_color = mix(f_color, v_color, hexagonContour());
    }

    f_color = mix(f_color, WHITE, lightness - 1.0f);
    f_color = mix(TRANSPARENT, f_color, opacity * 0.9f);
}
