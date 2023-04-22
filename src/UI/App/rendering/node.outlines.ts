import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import type Sigma from "sigma";
import vertexShaderSource from "./node.outlines.vert";
import fragmentShaderSource from "./node.outlines.frag";
import { floatColor } from "sigma/utils";
import { floatOutlineStyles, floatOutlineWidths, type Outlines } from "model/glyph";

export interface OutlinesNodeDisplayData extends NodeDisplayData {
    outlines: Outlines;
}

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_gap"];

export default function createOutlinesProgram(gap: number = 0.5): NodeProgramConstructor {
    return class extends OutlinesProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, gap);
        }
    };
}

export class OutlinesProgram extends NodeProgram<typeof UNIFORMS[number]> {
    constructor(gl: WebGLRenderingContext, renderer: Sigma, private gap: number = 0.5) {
        super(gl, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 6,
            VERTEX_SHADER_SOURCE: vertexShaderSource,
            FRAGMENT_SHADER_SOURCE: fragmentShaderSource,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_color", size: 4, type: UNSIGNED_BYTE, normalized: true },
                { name: "a_outlineWidths", size: 4, type: UNSIGNED_BYTE, normalized: true },
                { name: "a_outlineStyles", size: 4, type: UNSIGNED_BYTE, normalized: false }
            ],
        };
    }

    processVisibleItem(i: number, data: OutlinesNodeDisplayData): void {
        const array = this.array;

        array[i++] = data.x;
        array[i++] = data.y;
        array[i++] = data.size;
        array[i++] = floatColor(data.color || "#000000");
        array[i++] = floatOutlineWidths(data.outlines);
        array[i++] = floatOutlineStyles(data.outlines);
    }

    draw(params: RenderParams): void {
        const gl = this.gl;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_gap} = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_gap, this.gap);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);
    }
}
