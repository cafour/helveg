import { NodeProgram } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import type { IconAtlas } from "./iconAtlas";
import type Sigma from "sigma";
import vertexShaderSource from "./node.glyph.vert";
import fragmentShaderSource from "./node.glyph.frag";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

declare const UNIFORMS: readonly ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_atlas"];

export class GlyphProgram extends NodeProgram<typeof UNIFORMS[number]> {
    texture: WebGLTexture;

    constructor(gl: WebGLRenderingContext, renderer: Sigma, iconAtlas: IconAtlas) {
        super(gl, renderer);

        this.texture = gl.createTexture() as WebGLTexture;
        iconAtlas.redrawn.subscribe(a => {
            this.rebindTexture(a);
            this.renderer.scheduleRefresh();
        });
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 8,
            VERTEX_SHADER_SOURCE: vertexShaderSource,
            FRAGMENT_SHADER_SOURCE: fragmentShaderSource,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_color", size: 4, type: UNSIGNED_BYTE, normalized: true },
                { name: "a_texture", size: 4, type: FLOAT },
            ],
        };
    }

    processVisibleItem(i: number, data: NodeDisplayData): void {
        throw new Error("Method not implemented.");
    }

    draw(params: RenderParams): void {
        throw new Error("Method not implemented.");
    }


    private rebindTexture(iconAtlas: IconAtlas) {
        const gl = this.gl;

        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, iconAtlas.texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.generateMipmap(gl.TEXTURE_2D);
    }
}
