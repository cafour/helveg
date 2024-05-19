#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;
in float a_intensity;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;
uniform float u_time;
uniform float u_normalizationRatio;

out vec4 v_color;

const vec4 LOW_COLOR = vec4(0.8, 0.0, 0.0, 0.1);
const vec4 MID_COLOR = vec4(1.0, 0.5, 0.0, 0.8);
const vec4 TOP_COLOR = vec4(0.0, 0.0, 0.0, 0.8);

// The sice of a particle relative to the size of the node.
const float PARTICLE_SCALE = 0.25;

// The ratio of the fires' width to its height.
const float ASPECT_RATIO = 0.5;

const float SPEED = 2.0;

struct Emitter {
    vec2 position;
    float radius;
};

// Based on https://thebookofshaders.com/10/
float rand(float v)
{
    return fract(sin(v) * 69301.11);
}

// Based on https://thebookofshaders.com/11/
float noise(float v)
{
    float i = floor(v);
    float f = fract(v);
    return mix(rand(i), rand(i + 1.0), smoothstep(0.0, 1.0, f));
}

float modHeight(float v, float height)
{
    return v - floor(v * (1.0 / height)) * height;
}

/**
* Returns a position of the current fire particle relative to the center of the fire.
* The 'x' coordinate is in [-1, 1] and the 'y' coordinate is in [-1 / ASPECT_RATIO, 1 / ASPECT_RATIO].
*/
vec4 fire()
{
    // The maximum fire height.
    float maxHeight = 1.0 / ASPECT_RATIO * 2.0;

    // Snowflake is essentially the 'index' as a float between 0 and 1.
    float snowflake = rand(float(gl_InstanceID) * float(gl_VertexID));

    // The length of this snowflake's life. Causes the fire to look more natural since the flakes don't die out
    // all at once.
    float life = 1.0 - 0.2 * snowflake;

    // The maximum height of this particular snowflake.
    float height = life * maxHeight;

    // The offset at which this snowflake starts when u_time = 0.
    float initialHeight = snowflake * height;

    float velocity = (snowflake + 1.0) / 2.0 * SPEED * a_intensity;

    // The current height of the snowflake. It resets once it reaches its maximum height.
    float offsetY = modHeight(initialHeight + u_time * velocity, height);

    float lateral = pow(smoothstep(0.0, height, offsetY), 0.25);
    float angle = 3.14159 * (snowflake * 2.0 - 1.0);
    float wiggleX = (noise(snowflake * 123.99 + u_time) * 2.0 - 1.0) * 0.2;
    // float wiggleZ = (noise(gl_LocalInvocationID.z * 98.2002 + u_time) * 2.0 - 1.0) * 2.0;
    float offsetX = (cos(angle) + wiggleX) * lateral * a_intensity;
    // float offsetZ = sin(angle) * radius * lateral + wiggleZ;
    
    return vec4(offsetX, offsetY, offsetY / height, snowflake);
}

void main()
{
    if (a_intensity < 0.001) {
        gl_PointSize = 0.0;
        return;
    }

    vec4 fireResult = fire();
    vec2 fireOffset = fireResult.xy * (a_size / u_sizeRatio) * u_normalizationRatio;

    float nodeSize = a_size / u_sizeRatio * u_pixelRatio;
    gl_PointSize = nodeSize * PARTICLE_SCALE * a_intensity * (1.0 - fireResult.w * 0.5);


    gl_Position = vec4(
        (u_matrix * vec3(a_position + fireOffset, 1.0)).xy,
        0,
        1
    );

    if (a_intensity < 1.0) {
        v_color = TOP_COLOR;
        v_color.a = fireResult.z;
        return;
    }

    v_color = mix(
        mix(
            LOW_COLOR,
            MID_COLOR,
            smoothstep(0.0, 0.3, fireResult.z)),
        TOP_COLOR,
        smoothstep(0.3, 1.0, fireResult.z));
}
