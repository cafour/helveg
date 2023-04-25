import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import type Sigma from "sigma";
import vertexShaderSource from "./node.outlines.vert";
import fragmentShaderSource from "./node.outlines.frag";
import { floatColor } from "sigma/utils";
import { floatOutlineStyles, floatOutlineWidths, type Outlines } from "model/glyph";
import { StructuralDiagramMode, type HelvegNodeAttributes } from "model/structural";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_gap"];

export interface OutlinesProgramOptions {
    gap: number;
    diagramMode: StructuralDiagramMode;
}

export const DEFAULT_OUTLINES_PROGRAM_OPTIONS: OutlinesProgramOptions = {
    gap: 0,
    diagramMode: StructuralDiagramMode.Normal
};

export default function createOutlinesProgram(options?: Partial<OutlinesProgramOptions>): NodeProgramConstructor {
    if (options === undefined) {
        options = DEFAULT_OUTLINES_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_OUTLINES_PROGRAM_OPTIONS);
    }

    return class extends OutlinesProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, options as OutlinesProgramOptions);
        }
    };
}

export class OutlinesProgram extends NodeProgram<typeof UNIFORMS[number]> {
    constructor(gl: WebGLRenderingContext, renderer: Sigma, private options: OutlinesProgramOptions) {
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

    processVisibleItem(i: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        const useColor = this.options.diagramMode === StructuralDiagramMode.Normal
            || data.highlighted === true;
        
        array[i++] = data.x;
        array[i++] = data.y;
        array[i++] = data.size;
        array[i++] = floatColor(useColor ? data.color || "#000000" : "#aaaaaa");
        array[i++] = floatOutlineWidths(data.outlines);
        array[i++] = floatOutlineStyles(data.outlines);
    }

    draw(params: RenderParams): void {
        const gl = this.gl;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_gap} = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_gap, this.options.gap);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);
    }
}
