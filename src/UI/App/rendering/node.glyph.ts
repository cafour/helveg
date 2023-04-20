import { AbstractNodeProgram, NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import { IconAtlas, IconAtlasEntryStatus } from "./iconAtlas";
import type Sigma from "sigma";
import vertexShaderSource from "./node.glyph.vert";
import fragmentShaderSource from "./node.glyph.frag";
import { getIconDataUrl } from "model/icons";
import { floatColor } from "sigma/utils";

interface GlyphNodeDisplayData extends NodeDisplayData {
    icon: string;
}

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_atlas"];

export default function createGlyphProgram(iconAtlas: IconAtlas): NodeProgramConstructor {
    return class extends GlyphProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, iconAtlas);
        }
    };
}

export class GlyphProgram extends NodeProgram<typeof UNIFORMS[number]> {
    texture: WebGLTexture;

    constructor(gl: WebGLRenderingContext, renderer: Sigma, private iconAtlas: IconAtlas) {
        super(gl, renderer);

        this.texture = gl.createTexture() as WebGLTexture;
        iconAtlas.redrawn.subscribe(a => {
            this.rebindTexture(a);
            this.renderer.scheduleRefresh();
        });
        this.rebindTexture(iconAtlas);
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

    processVisibleItem(i: number, data: GlyphNodeDisplayData): void {
        const array = this.array;
        this.iconAtlas.tryAddIcon(data.icon);

        array[i++] = data.x;
        array[i++] = data.y;
        array[i++] = data.size;
        array[i++] = floatColor(data.color || "#000000");

        let atlasEntry = this.iconAtlas.entries[data.icon];
        if (atlasEntry && atlasEntry.status === IconAtlasEntryStatus.Rendered) {
            array[i++] = atlasEntry.x / this.iconAtlas.width;
            array[i++] = atlasEntry.y / this.iconAtlas.height;
            array[i++] = this.iconAtlas.iconSize / this.iconAtlas.width;
            array[i++] = this.iconAtlas.iconSize / this.iconAtlas.height;
        } else {
            // the icon is not ready yet, so don't render it
            array[i++] = 0;
            array[i++] = 0;
            array[i++] = 0;
            array[i++] = 0;
        }
    }

    draw(params: RenderParams): void {
        const gl = this.gl;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_atlas } = this.uniformLocations;

        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1i(u_atlas, 0);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);
    }


    private rebindTexture(iconAtlas: IconAtlas) {
        const gl = this.gl;

        gl.bindTexture(gl.TEXTURE_2D, this.texture);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, iconAtlas.texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR_MIPMAP_LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.NEAREST);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.generateMipmap(gl.TEXTURE_2D);
    }
}
