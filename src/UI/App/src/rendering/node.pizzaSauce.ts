import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { RenderParams } from "sigma/types";
import type Sigma from "sigma";
import vertexShaderSource from "./node.pizzaSauce.vert";
import fragmentShaderSource from "./node.pizzaSauce.frag";
import type { HelvegNodeAttributes } from "model/graph";
import type { PizzaProgramOptions } from "./pizza";

/*
** PIZZA TERMINOLOGY **
- crust: the outer edge of the pizza
- toppings: the ingredients on top of the pizza
- dough: the basic element of pizza structure prior to baking
- sauce: the base layer on top of the dough
*/

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_sauceWidth", "u_offset", "u_resolution", "u_zoomRatio"];

export class PizzaSauceProgram extends NodeProgram<typeof UNIFORMS[number]> {
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

        const { u_sizeRatio, u_pixelRatio, u_matrix, u_sauceWidth, u_offset, u_resolution, u_zoomRatio } = this.uniformLocations;

        gl.uniform1f(u_sizeRatio, params.sizeRatio);
        gl.uniform1f(u_pixelRatio, params.pixelRatio);
        gl.uniformMatrix3fv(u_matrix, false, params.matrix);
        gl.uniform1f(u_sauceWidth, this.options.sauceWidth);
        let offset = this.renderer.graphToViewport(this.renderer.getCamera());
        gl.uniform2f(u_offset, -offset.x, offset.y);
        gl.uniform2f(u_resolution, params.width, params.height);
        
        // TODO: Figure out how all these Sigma ratios work together.
        gl.uniform1f(u_zoomRatio, params.zoomRatio);

        gl.drawArrays(gl.POINTS, 0, this.verticesCount);
    }
}
