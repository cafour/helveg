#version 300 es

precision mediump float;

in vec4 v_texture;
in float v_size;
in vec2 v_rotation;
in vec4 v_id;

out vec4 f_color;

uniform sampler2D u_atlas;

void main(void) {
    vec2 texCoord = (gl_PointCoord.xy - 0.5) * v_rotation + 0.5;
    if (texCoord.x < 0.02 || texCoord.x > 0.98 || texCoord.y < 0.02 || texCoord.y > 0.98) {
        // get rid of artifacts at the edge of the texture
        discard;
    }
    vec4 texel = texture(u_atlas, v_texture.xy + texCoord * v_texture.zw);
    #ifdef PICKING_MODE
    f_color = v_id * texel.a;
    #else
    f_color = texel;
    #endif
}
