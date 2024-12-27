import { ProgramDefinition, Sigma, RenderParams, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { FireStatus } from "../model/style.ts";
import vertSrc from "./shaders/node.fire.vert";
import fragSrc from "./shaders/node.fire.frag";
import { HelvegNodeProgramType, HelvegNodeProgram } from "../diagram/initializers.ts";

export interface FireProgramOptions {
    showOnlyHighlighted: boolean;
    particleCount: number;
    isFireAnimated: boolean;
}

export const DEFAULT_FIRE_PROGRAM_OPTIONS: FireProgramOptions = {
    showOnlyHighlighted: false,
    particleCount: 32,
    isFireAnimated: true,
};

export default function createFireProgram(options?: Partial<FireProgramOptions>): HelvegNodeProgramType {
    if (options === undefined) {
        options = DEFAULT_FIRE_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_FIRE_PROGRAM_OPTIONS);
    }

    return class extends FireProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
            super(gl, pickingBuffer, renderer, options as FireProgramOptions);
        }
    };
}

const { UNSIGNED_BYTE, FLOAT } = WebGL2RenderingContext;
const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_time", "u_normalizationRatio"];

export class FireProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: Sigma,
        private options: FireProgramOptions
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
                { name: "a_intensity", size: 1, type: FLOAT }
            ],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        const useIntensity = !this.options.showOnlyHighlighted || data.highlighted === true;

        let intensity = data.fire === FireStatus.Smoke ? 0.5
            : data.fire === FireStatus.Flame ? 1
                : 0;

        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = data.size ?? 2;
        array[offset++] = useIntensity ? intensity : 0;
    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const { gl, uniformLocations } = programInfo;
        const { u_sizeRatio, u_pixelRatio, u_matrix, u_time, u_normalizationRatio } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_time, this.options.isFireAnimated ? performance.now() / 1000.0 : 42.0);
        gl.uniform1f(u_normalizationRatio,
            (1.0 / this.renderer.getGraphToViewportRatio())
            / (this.renderer as any).normalizationFunction.ratio * 0.90);
    }

    drawWebGL(method: number, programInfo: ProgramInfo): void {
        const gl = programInfo.gl as WebGL2RenderingContext;
        gl.drawArraysInstanced(method, 0, this.verticesCount, this.options.particleCount);
    }
}
