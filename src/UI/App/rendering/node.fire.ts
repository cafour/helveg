import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { RenderParams } from "sigma/types";
import type Sigma from "sigma";
import { StructuralDiagramMode, type HelvegNodeAttributes } from "model/structural";
import vertexShaderSource from "./node.fire.vert";
import fragmentShaderSource from "./node.fire.frag";
import { FireStatus } from "model/glyph";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_time"];

export interface FireProgramOptions {
    diagramMode: StructuralDiagramMode;
    particleCount: number;
}

export const DEFAULT_OUTLINES_PROGRAM_OPTIONS: FireProgramOptions = {
    diagramMode: StructuralDiagramMode.Normal,
    particleCount: 32
};

export default function createFireProgram(options?: Partial<FireProgramOptions>): NodeProgramConstructor {
    if (options === undefined) {
        options = DEFAULT_OUTLINES_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_OUTLINES_PROGRAM_OPTIONS);
    }

    return class extends FireProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, options as FireProgramOptions);
        }
    };
}

export class FireProgram extends NodeProgram<typeof UNIFORMS[number]> {
    constructor(gl: WebGLRenderingContext, renderer: Sigma, private options: FireProgramOptions) {
        super(gl, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 4,
            VERTEX_SHADER_SOURCE: vertexShaderSource,
            FRAGMENT_SHADER_SOURCE: fragmentShaderSource,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
                { name: "a_intensity", size: 1, type: FLOAT },
            ],
        };
    }

    processVisibleItem(i: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        const useIntensity = this.options.diagramMode === StructuralDiagramMode.Normal
            || data.highlighted === true;

        let intensity = data.fire === FireStatus.Smoke ? 0.5
            : data.fire === FireStatus.Flame ? 1
                : 0;

        array[i++] = data.x;
        array[i++] = data.y;
        array[i++] = data.size;
        array[i++] = useIntensity ? intensity : 0;
    }

    draw(params: RenderParams): void {
        const gl = this.gl as WebGL2RenderingContext;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_time } = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_time, performance.now() / 1000.0);

        gl.drawArraysInstanced(gl.POINTS, 0, this.verticesCount, this.options.particleCount);
    }
}
