#version 300 es

precision mediump float;

in vec4 v_texture;
in vec2 v_texCoord;
in vec4 v_color;

out vec4 f_color;

uniform sampler2D u_atlas;
uniform float u_invert;

void main(void) {
    if (v_texCoord.x < 0.0f || v_texCoord.y < 0.0f || v_texCoord.x > 1.0f || v_texCoord.y > 1.0f) {
        discard;
    }

    // NB: the 2% is a hack to avoid artifacts at the edges of icons in an atlas
    if (v_texCoord.x > 0.01f && v_texCoord.x < 0.99f && v_texCoord.y > 0.01f && v_texCoord.y < 0.99f) {
        vec4 texel = texture(u_atlas, v_texture.xy + v_texCoord * v_texture.zw);
        if (texel.a == 0.0f) {
            discard;
        }

        if (u_invert > 0.0f) {
            texel.rgb = vec3(1.0f) - texel.rgb;
        }

        f_color = mix(f_color, v_color, texel.a);
    }
}
