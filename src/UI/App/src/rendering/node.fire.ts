import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { RenderParams } from "sigma/types";
import type Sigma from "sigma";
import { StructuralDiagramMode } from "model/structural";
import vertexShaderSource from "./node.fire.vert";
import fragmentShaderSource from "./node.fire.frag";
import type { HelvegNodeAttributes } from "model/graph";
import { FireStatus } from "model/style";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_time", "u_normalizationRatio"];

export interface FireProgramOptions {
    diagramMode: StructuralDiagramMode;
    particleCount: number;
    showFire: boolean;
    isFireAnimated: boolean;
}

export const DEFAULT_FIRE_PROGRAM_OPTIONS: FireProgramOptions = {
    diagramMode: StructuralDiagramMode.Normal,
    particleCount: 32,
    showFire: true,
    isFireAnimated: true,
};

export default function createFireProgram(options?: Partial<FireProgramOptions>): NodeProgramConstructor {
    if (options === undefined) {
        options = DEFAULT_FIRE_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_FIRE_PROGRAM_OPTIONS);
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
                { name: "a_intensity", size: 1, type: FLOAT }
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

        array[i++] = data.x ?? 0;
        array[i++] = data.y ?? 0;
        array[i++] = data.size ?? 2;
        array[i++] = useIntensity ? intensity : 0;
    }

    draw(params: RenderParams): void {
        const gl = this.gl as WebGL2RenderingContext;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_time, u_normalizationRatio } = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_time, this.options.isFireAnimated ? performance.now() / 1000.0 : 42.0);
        gl.uniform1f(u_normalizationRatio,
            (1.0 / this.renderer.getGraphToViewportRatio())
                / (this.renderer as any).normalizationFunction.ratio * 0.90);

        gl.drawArraysInstanced(gl.POINTS, 0, this.verticesCount, this.options.particleCount);
    }
}
