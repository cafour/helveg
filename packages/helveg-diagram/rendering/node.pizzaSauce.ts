import { ProgramDefinition, RenderParams, ProgramInfo } from "../deps/sigma.ts";
import { HelvegNodeAttributes, HelvegNodeProgram, HelvegSigma } from "../model/graph.ts";
import { PizzaProgramOptions } from "./pizza.ts";
import vertSrc from "./shaders/node.pizzaSauce.vert";
import fragSrc from "./shaders/node.pizzaSauce.frag";

/*
** PIZZA TERMINOLOGY **
- crust: the outer edge of the pizza
- toppings: the ingredients on top of the pizza
- dough: the basic element of pizza structure prior to baking
- sauce: the base layer on top of the dough
*/

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_sauceWidth", "u_offset", "u_resolution", "u_zoomRatio"];

export class PizzaSauceProgram extends HelvegNodeProgram<typeof UNIFORMS[number]> {
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
        const {
            u_sizeRatio,
            u_pixelRatio,
            u_matrix,
            u_sauceWidth,
            u_offset,
            u_resolution,
            u_zoomRatio
        } = uniformLocations;
        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_sauceWidth, this.options.sauceWidth);
        let offset = this.renderer.graphToViewport(this.renderer.getCamera());
        gl.uniform2f(u_offset, -offset.x, offset.y);
        gl.uniform2f(u_resolution, params.width, params.height);

        // TODO: Figure out how all these Sigma ratios work together.
        gl.uniform1f(u_zoomRatio, params.zoomRatio);
    }

    protected renderProgram(params: RenderParams, programInfo: ProgramInfo): void {
        if (programInfo.isPicking) {
            return;
        }

        super.renderProgram(params, programInfo);
    }
}
