import { Sigma, ProgramDefinition, RenderParams, floatColor, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { FALLBACK_NODE_STYLE, floatOutlineWidths, floatOutlineStyles } from "../model/style.ts";
import vertSrc from "./shaders/node.outlines.vert";
import fragSrc from "./shaders/node.outlines.frag";
import { HelvegNodeProgram, HelvegNodeProgramType } from "../diagram/initializers.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_gap"];

export interface OutlinesProgramOptions {
    gap: number;
    showOnlyHighlighted: boolean;
    showCollapsedNodeIndicators: boolean;
}

export const DEFAULT_OUTLINES_PROGRAM_OPTIONS: OutlinesProgramOptions = {
    gap: 0,
    showOnlyHighlighted: false,
    showCollapsedNodeIndicators: true
};

export default function createOutlinesProgram(options?: Partial<OutlinesProgramOptions>): HelvegNodeProgramType {
    if (options === undefined) {
        options = DEFAULT_OUTLINES_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_OUTLINES_PROGRAM_OPTIONS);
    }

    return class extends OutlinesProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
            super(gl, pickingBuffer, renderer, options as OutlinesProgramOptions);
        }
    };
}

export class OutlinesProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: OutlinesProgramOptions
    ) {
        super(gl, pickingBuffer, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            METHOD: WebGL2RenderingContext.POINTS,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_color", size: 4, type: UNSIGNED_BYTE, normalized: true },
                { name: "a_outlineWidths", size: 4, type: UNSIGNED_BYTE, normalized: true },
                { name: "a_outlineStyles", size: 4, type: UNSIGNED_BYTE, normalized: false },
                { name: "a_collapsed", size: 1, type: FLOAT },
                { name: "a_id", size: 4, type: UNSIGNED_BYTE, normalized: true },
            ],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        const useColor = !this.options.showOnlyHighlighted || data.highlighted === true;

        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = data.size ?? 2;
        array[offset++] = floatColor(useColor ? data.color ?? FALLBACK_NODE_STYLE.color : "#aaaaaa");
        array[offset++] = floatOutlineWidths(data.outlines ?? FALLBACK_NODE_STYLE.outlines);
        array[offset++] = floatOutlineStyles(data.outlines ?? FALLBACK_NODE_STYLE.outlines);
        array[offset++] = +!!(useColor && this.options.showCollapsedNodeIndicators && data.collapsed);
        array[offset++] = nodeIndex;
    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const {gl, uniformLocations} = programInfo;
        const { u_sizeRatio, u_pixelRatio, u_matrix, u_gap } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_gap, this.options.gap);
    }
    
    drawWebGL(method: number, programInfo: ProgramInfo): void {
        const {gl} = programInfo;
        if (programInfo.isPicking) {
            gl.blendFunc(gl.ONE, gl.ZERO);
        } else {
            gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
        }

        super.drawWebGL(method, programInfo);
    }
}
