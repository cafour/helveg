import { createNodeCompoundProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type Sigma from "sigma";
import { PizzaDoughProgram } from "./node.pizzaDough";
import { PizzaSauceProgram } from "./node.pizzaSauce";

/*
** PIZZA TERMINOLOGY **
- crust: the outer edge of the pizza
- toppings: the ingredients on top of the pizza
- dough: the basic element of pizza structure prior to baking
- sauce: the base layer on top of the dough
*/

const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_crustWidth", "u_sauceWidth"];

export interface PizzaProgramOptions {
    isPizzaEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
}

export const DEFAULT_PIZZA_PROGRAM_OPTIONS: PizzaProgramOptions = {
    isPizzaEnabled: false,
    crustWidth: 20,
    sauceWidth: 40
};

export default function createPizzaProgram(options?: Partial<PizzaProgramOptions>): NodeProgramConstructor {
    if (options === undefined) {
        options = DEFAULT_PIZZA_PROGRAM_OPTIONS;
    } else {
        Object.assign(options, DEFAULT_PIZZA_PROGRAM_OPTIONS);
    }

    return createNodeCompoundProgram([
        class extends PizzaDoughProgram {
            constructor(gl: WebGLRenderingContext, renderer: Sigma) {
                super(gl, renderer, options as PizzaProgramOptions);
            }
        },
        class extends PizzaSauceProgram {
            constructor(gl: WebGLRenderingContext, renderer: Sigma) {
                super(gl, renderer, options as PizzaProgramOptions);
            }
        },
    ]);
}
