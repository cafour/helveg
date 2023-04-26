#version 300 es

precision mediump float;

in vec2 a_position;
in float a_size;
in float a_intensity;

uniform float u_sizeRatio;
uniform float u_pixelRatio;
uniform mat3 u_matrix;
uniform float u_time;

out vec4 v_color;

const vec4 LOW_COLOR = vec4(0.8, 0.0, 0.0, 1.0);
const vec4 MID_COLOR = vec4(1.0, 0.5, 0.0, 1.0);
const vec4 TOP_COLOR = vec4(0.0, 0.0, 0.0, 1.0);
const float SCALE = 0.1;

struct Particle {
    vec3 position;
    float pad;
    vec4 color;
};

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

void main()
{
    if (a_intensity < 0.001) {
        gl_PointSize = 0.0;
        return;
    }
    
    Emitter emitter;
    emitter.position = a_position;
    emitter.radius = (a_size * SCALE) / u_sizeRatio * u_pixelRatio * 2.0;
    gl_PointSize = emitter.radius * a_intensity;

    float maxHeight = 4.0 * emitter.radius;
    float index = float(gl_InstanceID);
    float snowflake = rand(float(index));

    float life = (1.0 - pow(snowflake, 4.0));

    float height = life * maxHeight;
    float initialHeight = snowflake * height;
    float velocity = (snowflake + 1.0) * 4.0;
    float offsetY = modHeight(initialHeight + u_time * velocity, height);

    float lateral = pow(smoothstep(0.0, height, offsetY), 0.25) * life;
    float radius = pow(rand(index), 2.0) * emitter.radius;
    float angle = 3.14159 * (rand(index) * 2.0 - 1.0);
    float wiggleX = (noise(index * 123.99 + u_time) * 2.0 - 1.0) * 2.0;
    // float wiggleZ = (noise(gl_LocalInvocationID.z * 98.2002 + u_time) * 2.0 - 1.0) * 2.0;
    float offsetX = cos(angle) * radius * lateral + wiggleX;
    // float offsetZ = sin(angle) * radius * lateral + wiggleZ;

    gl_Position = vec4((u_matrix * vec3(emitter.position + vec2(offsetX, offsetY), 1)).xy, 0, 1);

    v_color = mix(
        mix(
            LOW_COLOR,
            MID_COLOR,
            smoothstep(0.0, height * 0.3, offsetY)),
        TOP_COLOR,
        smoothstep(height * 0.3, height, offsetY));
}
