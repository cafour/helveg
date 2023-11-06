import { NodeProgram, Sigma, ProgramDefinition, RenderParams } from "../deps/sigma.ts";
import { HelvegNodeAttributes } from "../model/graph.ts";
import { PizzaProgramOptions } from "./pizza.ts";

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_crustWidth", "u_sauceWidth"];

export class PizzaDoughProgram extends NodeProgram<typeof UNIFORMS[number]> {
    constructor(gl: WebGLRenderingContext, renderer: Sigma, private options: PizzaProgramOptions) {
        super(gl, renderer);
    }

    getDefinition(): ProgramDefinition<typeof UNIFORMS[number]> {
        return {
            VERTICES: 1,
            ARRAY_ITEMS_PER_VERTEX: 3,
            VERTEX_SHADER_SOURCE: vertexShaderSource,
            FRAGMENT_SHADER_SOURCE: fragmentShaderSource,
            UNIFORMS,
            ATTRIBUTES: [
                { name: "a_position", size: 2, type: FLOAT },
                { name: "a_size", size: 1, type: FLOAT },
            ],
        };
    }

    processVisibleItem(i: number, data: HelvegNodeAttributes): void {
        const array = this.array;

        array[i++] = data.x ?? 0;
        array[i++] = data.y ?? 0;
        array[i++] = data.size ?? 2;
    }

    draw(params: RenderParams): void {
        const gl = this.gl as WebGL2RenderingContext;

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_crustWidth, u_sauceWidth } = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_crustWidth, this.options.crustWidth);
        gl.uniform1f(u_sauceWidth, this.options.sauceWidth);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);
    }
}
