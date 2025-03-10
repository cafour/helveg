import { Sigma, ProgramDefinition, RenderParams, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes, HelvegNodeProgram, HelvegSigma } from "../model/graph.ts";
import { PizzaProgramOptions } from "./pizza.ts";
import vertSrc from "./shaders/node.pizzaDough.vert";
import fragSrc from "./shaders/node.pizzaDough.frag";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_crustWidth", "u_sauceWidth"];

export class PizzaDoughProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: HelvegSigma,
        private options: PizzaProgramOptions
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
            ],
        };
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        array[offset++] = data.x ?? 0;
        array[offset++] = data.y ?? 0;
        array[offset++] = data.size ?? 2;
    }

    setUniforms(params: RenderParams, programInfo: ProgramInfo): void {
        const { gl, uniformLocations } = programInfo;
        const { u_sizeRatio, u_pixelRatio, u_matrix, u_crustWidth, u_sauceWidth } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_crustWidth, this.options.crustWidth);
        gl.uniform1f(u_sauceWidth, this.options.sauceWidth);
    }

    protected renderProgram(params: RenderParams, programInfo: ProgramInfo): void {
        if (programInfo.isPicking) {
            return;
        }

        super.renderProgram(params, programInfo);
    }
}
