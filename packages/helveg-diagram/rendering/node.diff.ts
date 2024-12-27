import { Sigma, ProgramDefinition, RenderParams, floatColor, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import vertSrc from "./shaders/node.diff.vert";
import fragSrc from "./shaders/node.diff.frag";
import { MultigraphNodeDiffStatus } from "../global.ts";
import { HelvegNodeProgramType, HelvegNodeProgram } from "../diagram/initializers.ts";

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

export default function createDiffProgram(options: DiffProgramOptions): HelvegNodeProgramType {
    return class extends DiffProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
            super(gl, pickingBuffer, renderer, options);
        }
    };
}

const { UNSIGNED_BYTE, FLOAT } = WebGL2RenderingContext;
const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix"];

export class DiffProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {

    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: DiffProgramOptions,
    ) {
        super(gl, pickingBuffer, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            METHOD: WebGLRenderingContext.POINTS,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_diffColor", size: 4, type: UNSIGNED_BYTE, normalized: true }
            ],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const isEnabled = !this.options.showOnlyHighlighted || data.highlighted === true;

        const array = this.array;
        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = isEnabled ? (data.size ?? 1) : 0;
        array[offset++] = floatColor(this.options.colors[data.diff ?? "unmodified"]);
    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const { gl, uniformLocations } = programInfo;
        const { u_sizeRatio, u_pixelRatio, u_matrix } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
    }

    drawWebGL(method: number, programInfo: ProgramInfo): void {
        const { gl } = programInfo;

        gl.blendFunc(gl.SRC_ALPHA, gl.DST_ALPHA);
        super.drawWebGL(method, programInfo);
        gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
    }
    
    protected renderProgram(params: RenderParams, programInfo: ProgramInfo): void {
        if (programInfo.isPicking) {
            return;
        }

        super.renderProgram(params, programInfo);
    }
}
