import { NodeProgramConstructor, Sigma, NodeProgram, ProgramDefinition, RenderParams, floatColor } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import vertSrc from "./shaders/node.diff.vert";
import fragSrc from "./shaders/node.diff.frag";
import { MultigraphNodeDiffStatus } from "../global.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix"];

export interface DiffProgramOptions {
    colors: Record<MultigraphNodeDiffStatus, string>;
    showOnlyHighlighted: boolean;
}

export const DEFAULT_DIFF_PROGRAM_OPTIONS: DiffProgramOptions = {
    colors: {
        unmodified: "#00000000",
        modified: "#999900aa",
        added: "#009900aa",
        deleted: "#990000aa"
    },
    showOnlyHighlighted: false
};

export default function createDiffProgram(options: DiffProgramOptions): NodeProgramConstructor {
    return class extends DiffProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, options);
        }
    };
}

export class DiffProgram extends NodeProgram<typeof UNIFORMS[number]> {

    constructor(gl: WebGLRenderingContext, renderer: Sigma, private options: DiffProgramOptions) {
        super(gl, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 4,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_diffColor", size: 4, type: UNSIGNED_BYTE, normalized: true }
            ],
        };
    }

    processVisibleItem(i: number, data: HelvegNodeAttributes): void {
        const isEnabled = !this.options.showOnlyHighlighted || data.highlighted === true;

        const array = this.array;
        array[i++] = data.x ?? 0;
        array[i++] = data.y ?? 0;
        array[i++] = isEnabled ? (data.size ?? 1) : 0;
        array[i++] = floatColor(this.options.colors[data.diff ?? "unmodified"]);
    }

    draw(params: RenderParams): void {
        const gl = this.gl;
        
        const { u_sizeRatio, u_pixelRatio, u_matrix } = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);

        gl.blendFunc(gl.SRC_ALPHA, gl.DST_ALPHA);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);

        gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
    }
}
