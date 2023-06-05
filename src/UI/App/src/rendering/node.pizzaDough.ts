import { NodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { ProgramDefinition } from "sigma/rendering/webgl/programs/common/program";
import type { RenderParams } from "sigma/types";
import type Sigma from "sigma";
import vertexShaderSource from "./node.pizzaDough.vert";
import fragmentShaderSource from "./node.pizzaDough.frag";
import type { HelvegNodeAttributes } from "model/graph";
import type { PizzaProgramOptions } from "./pizza";

/*
** PIZZA TERMINOLOGY **
- crust: the outer edge of the pizza
- toppings: the ingredients on top of the pizza
- dough: the basic element of pizza structure prior to baking
- sauce: the base layer on top of the dough
*/

/*
** PIZZA IMAGES**
- https://www.pexels.com/photo/sliced-pepperoni-pizza-on-white-ceramic-plate-708587/
- https://www.pexels.com/photo/top-view-photo-of-baked-pizza-2147491/
- https://www.pexels.com/photo/cooked-food-2471171/
- https://www.pexels.com/photo/italian-style-pizza-13814644/
- https://www.pexels.com/photo/italian-style-pizza-13985154/
- https://www.pexels.com/photo/close-up-view-of-pizza-with-olives-9844834/
- https://www.pexels.com/photo/pepperoni-pizza-803290/
- https://www.pexels.com/photo/a-person-holding-a-slice-of-pizza-on-a-wooden-board-14334060/
- https://www.pexels.com/photo/corn-kernels-on-strainer-1359315/
- https://www.pexels.com/photo/dill-pickles-in-a-bowl-and-crackers-on-a-white-surface-11622727/
- https://www.pexels.com/photo/1-piece-sliced-pizza-in-white-ceramic-plate-845808/
- https://www.pexels.com/photo/photo-of-cooked-bacon-4110377/
*/

/*
 1 [x] basil
 2 [x] mozzarella
 3 [x] jalapeno
 4 [x] cherry tomatoes
 5 [x] salami
 6 [x] pineapple
 7 [x] corn
 8 [x] ham
 9 [x] egg
10 [x] bacon
11 [ ] fries
12 [ ] chicken
13 [ ] spinach
14 [ ] mushrooms
15 [ ] olomucké syrečky
16 [ ] dried tomatoes
17 [ ] onion
18 [ ] paprika
19 [ ] shrimp
20 [ ] olives
*/

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
