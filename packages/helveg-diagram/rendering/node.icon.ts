import { NodeProgramConstructor, Sigma, NodeProgram, ProgramDefinition, RenderParams } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { StructuralDiagramMode } from "../model/structural.ts";
import { FALLBACK_NODE_ICON } from "../model/style.ts";
import { IconAtlas, IconAtlasEntryStatus } from "./iconAtlas.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_atlas"];

export interface IconProgramOptions {
    iconAtlas: IconAtlas;
    diagramMode: StructuralDiagramMode;
}

export default function createIconProgram(options: IconProgramOptions): NodeProgramConstructor {
    return class extends IconProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, options);
        }
    };
}

export class IconProgram extends NodeProgram<typeof UNIFORMS[number]> {
    texture: WebGLTexture;

    constructor(gl: WebGLRenderingContext, renderer: Sigma, private options: IconProgramOptions) {
        super(gl, renderer);

        this.texture = gl.createTexture() as WebGLTexture;
        options.iconAtlas.redrawn.subscribe(a => {
            this.rebindTexture(a);
            this.renderer.scheduleRefresh();
        });
        this.rebindTexture(options.iconAtlas);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 7,
            VERTEX_SHADER_SOURCE: vertexShaderSource,
            FRAGMENT_SHADER_SOURCE: fragmentShaderSource,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_iconSize", size: 1, type: FLOAT },
                { name: "a_texture", size: 4, type: FLOAT }
            ],
        };
    }

    processVisibleItem(i: number, data: HelvegNodeAttributes): void {
        const array = this.array;
        this.options.iconAtlas.tryAddIcon(data.icon ?? FALLBACK_NODE_ICON);

        const isVisible = (this.options.diagramMode === StructuralDiagramMode.Normal
            || data.highlighted === true);
        
        array[i++] = data.x ?? 0;
        array[i++] = data.y ?? 0;
        array[i++] = isVisible ? data.iconSize ?? data.size ?? 1 : 0;

        let atlasEntry = this.options.iconAtlas.entries[data.icon ?? FALLBACK_NODE_ICON];
        if (atlasEntry && atlasEntry.status === IconAtlasEntryStatus.Rendered) {
            array[i++] = atlasEntry.x / this.options.iconAtlas.width;
            array[i++] = atlasEntry.y / this.options.iconAtlas.height;
            array[i++] = this.options.iconAtlas.iconSize / this.options.iconAtlas.width;
            array[i++] = this.options.iconAtlas.iconSize / this.options.iconAtlas.height;
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
