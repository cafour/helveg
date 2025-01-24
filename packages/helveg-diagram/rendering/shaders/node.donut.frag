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

out vec4 f_color;

uniform float u_pixelRatio;

const vec4 transparent = vec4(0.0f, 0.0f, 0.0f, 0.0f);
const float pi = 3.14159f;
const float eps = 0.00001f;
const float strokedLightness = 1.3f;
const float solidLightness = 1.1f;

// Returns:
// * 1 if inside the circle
// * 0 if outside the circle
// * (1, 0) if on the border of the circle
float circle(float radius, float dist) {
    float distDelta = fwidth(dist);
    float t = 1.0f;
    // return dist > radius ? 0.0 : 1.0;
    float edge = dist - (radius - distDelta);
    if (edge > distDelta)
        t = 0.0f;
    else if (edge > 0.0f)
        t = 1.0f - edge / distDelta;
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

// float lightness(vec3 color) {
//     return 0.5f * (min(color.r, min(color.g, color.b)) + max(color.r, max(color.g, color.b)));
// }

vec3 rgb2hsl(in vec3 c) {
    float h = 0.0f;
    float s = 0.0f;
    float l = 0.0f;
    float r = c.r;
    float g = c.g;
    float b = c.b;
    float cMin = min(r, min(g, b));
    float cMax = max(r, max(g, b));

    l = (cMax + cMin) / 2.0f;
    if (cMax > cMin) {
        float cDelta = cMax - cMin;

        //s = l < .05 ? cDelta / ( cMax + cMin ) : cDelta / ( 2.0 - ( cMax + cMin ) ); Original
        s = l < .0f ? cDelta / (cMax + cMin) : cDelta / (2.0f - (cMax + cMin));

        if (r == cMax) {
            h = (g - b) / cDelta;
        } else if (g == cMax) {
            h = 2.0f + (b - r) / cDelta;
        } else {
            h = 4.0f + (r - g) / cDelta;
        }

        if (h < 0.0f) {
            h += 6.0f;
        }
        h = h / 6.0f;
    }
    return vec3(h, s, l);
}

vec3 hsl2rgb(in vec3 c) {
    vec3 rgb = clamp(abs(mod(c.x * 6.0f + vec3(0.0f, 4.0f, 2.0f), 6.0f) - 3.0f) - 1.0f, 0.0f, 1.0f);

    return c.z + c.y * (rgb - 0.5f) * (1.0f - abs(2.0f * c.z - 1.0f));
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

    float opacity = 0.0f;
    float lightnessFactor = 1.0f;
    float saturationFactor = 1.0f;

    float innerCircleFactor = circle(v_radii.x, dist);
    opacity += innerCircleFactor;

    f_color = v_color;

    if (dist < v_radii.x) {
        lightnessFactor = 1.5f;
        saturationFactor = 0.7f;
        f_color = v_backgroundColor;
    } else if (v_radii.y > 0.0f) {
        float donutFactor = circle(v_radii.y, dist) - circle(v_radii.x + v_gap, dist);
        opacity += donutFactor;

        // NB: x and y are switched on purpose (and y flipped as well) to emulate a rotation by 90 deg clockwise
        float halfAngle = abs(atan(v_diffVector.x, -v_diffVector.y));
        float margin = v_stroke;
        if (v_strokedSlice < eps) {
            // full solid
            lightnessFactor = solidLightness;
            f_color = v_color;
        } else if (pi - v_strokedSlice < eps) {
            // full stroked
            lightnessFactor = strokedLightness;
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
            lightnessFactor = strokedLightness * strokedSector + solidLightness * solidSector;
            saturationFactor = lightnessFactor;
            // top and bottom line of the stroked sector
            // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (circle(v_radii.x + v_gap + margin, dist) + 1.0f - circle(v_radii.y - margin, dist))));
            // left and right line of the stroked sector
            // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * (1.0f - sector(v_strokedSlice, halfAngle, dist, v_gap + 2.0f * margin))));
            // hatching
            // sectorFactor = max(sectorFactor, mix(0.0f, solidFactor, strokedSector * hatch()));
        }
    }

    // vec3 hsl = rgb2hsl(f_color.rgb);
    // hsl.g *= saturationFactor;
    // hsl.z *= lightnessFactor;
    // f_color.rgb = hsl2rgb(hsl);

    f_color = mix(transparent, f_color, opacity * 0.9f);
}
