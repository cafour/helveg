import {
    floatColor,
    InstancedProgramDefinition,
    ProgramInfo,
    RenderParams,
    Sigma,
} from "../deps/sigma.ts";
import {
    HelvegNodeProgram,
    HelvegNodeProgramType,
} from "../diagram/initializers.ts";
import vertSrc from "./shaders/node.donut.vert";
import fragSrc from "./shaders/node.donut.frag";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { FALLBACK_NODE_STYLE } from "../global.ts";

export interface DonutProgramOptions {
    gap: number;
    stroke: number;
    hatchingWidth: number;
    showOnlyHighlighted: boolean;
}

export const DEFAULT_DONUT_PROGRAM_OPTIONS: DonutProgramOptions = {
    gap: 1,
    stroke: 10,
    hatchingWidth: 16,
    showOnlyHighlighted: false,
};

export default function createDonutProgram(
    options?: Partial<DonutProgramOptions>,
): HelvegNodeProgramType {
    // NB: Cannot use options = {...DEFAULT_DONUT_PROGRAM_OPTIONS, ...options} because we need to keep the original
    //     object.
    if (options === undefined) {
        options = DEFAULT_DONUT_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_DONUT_PROGRAM_OPTIONS);
    }
    return class extends DonutProgram {
        constructor(
            gl: WebGLRenderingContext,
            pickingBuffer: WebGLFramebuffer,
            renderer: Sigma,
        ) {
            super(gl, pickingBuffer, renderer, options as DonutProgramOptions);
        }
    };
}

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;
const UNIFORMS = [
    "u_sizeRatio",
    "u_pixelRatio",
    "u_correctionRatio",
    "u_matrix",
    "u_gap",
    "u_stroke",
    "u_hatchingWidth",
];

export class DonutProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: DonutProgramOptions,
    ) {
        super(gl, pickingBuffer, renderer);
    }

    getDefinition(): InstancedProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 3,
            VERTEX_SHADER_SOURCE: vertSrc,
            FRAGMENT_SHADER_SOURCE: fragSrc,
            METHOD: WebGL2RenderingContext.TRIANGLES,
            UNIFORMS,
            ATTRIBUTES: [
                {
                    name: "a_id",
                    size: 4,
                    type: UNSIGNED_BYTE,
                    normalized: true,
                },
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_baseSize", size: 1, type: FLOAT },
                { name: "a_slices", size: 3, type: FLOAT },
                {
                    name: "a_color",
                    size: 4,
                    type: UNSIGNED_BYTE,
                    normalized: true,
                },
            ],
            // NB: Data for an equilateral triangle that the donut is carved from.
            CONSTANT_ATTRIBUTES: [{ name: "a_angle", size: 1, type: FLOAT }],
            CONSTANT_DATA: [[(0 * Math.PI) / 3], [(2 * Math.PI) / 3], [
                (4 * Math.PI) / 3,
            ]],
        };
    }

    processVisibleItem(
        nodeIndex: number,
        offset: number,
        data: HelvegNodeAttributes,
    ): void {
        const array = this.array;
        const useColor = !this.options.showOnlyHighlighted ||
            data.highlighted === true;

        array[offset++] = nodeIndex;
        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = data.baseSize ?? 2;
        array[offset++] = data.slices?.stroked ??
            FALLBACK_NODE_STYLE.slices.solid;
        array[offset++] = data.slices?.solid ??
            FALLBACK_NODE_STYLE.slices.stroked;
        array[offset++] = data.slices?.width ??
            FALLBACK_NODE_STYLE.slices.width;
        array[offset++] = floatColor(
            useColor ? data.color ?? FALLBACK_NODE_STYLE.color : "#aaaaaa",
        );
    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const { gl, uniformLocations } = programInfo;
        const {
            u_sizeRatio,
            u_pixelRatio,
            u_correctionRatio,
            u_matrix,
            u_gap,
            u_stroke,
            u_hatchingWidth,
        } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniform1f(u_correctionRatio, params.correctionRatio);
        gl.uniform1f(u_stroke, this.options.stroke);
        gl.uniform1f(u_hatchingWidth, this.options.hatchingWidth);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_gap, 1);
    }

    drawWebGL(method: number, programInfo: ProgramInfo): void {
        const { gl } = programInfo;
        if (programInfo.isPicking) {
            gl.blendFunc(gl.ONE, gl.ZERO);
        } else {
            gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
        }

        super.drawWebGL(method, programInfo);
    }
}
