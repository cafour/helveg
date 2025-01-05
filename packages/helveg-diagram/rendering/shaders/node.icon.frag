#version 300 es

precision mediump float;

in vec4 v_texture;
in vec2 v_texCoord;
flat in float v_grayscale;

out vec4 f_color;

uniform sampler2D u_atlas;

void main(void) {
    if (v_texCoord.x < 0.0f || v_texCoord.y < 0.0f || v_texCoord.x > 1.0f || v_texCoord.y > 1.0f) {
        discard;
    }

    // NB: the 2% is a hack to avoid artifacts at the edges of icons in an atlas
    if (v_texCoord.x > 0.01f && v_texCoord.x < 0.99f && v_texCoord.y > 0.01f && v_texCoord.y < 0.99f) {
        vec4 texel = texture(u_atlas, v_texture.xy + v_texCoord * v_texture.zw);
        f_color = texel;
        if (v_grayscale > 0.0f) {
            // NB: The 2.0f is magical to make the icons less contrasting.
            f_color.rgb = 2.0f * vec3(f_color.r + f_color.g + f_color.b) / 3.0f;
        }
    }
}
