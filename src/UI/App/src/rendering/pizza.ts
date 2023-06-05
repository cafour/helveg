import { createNodeCompoundProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type Sigma from "sigma";
import { PizzaDoughProgram } from "./node.pizzaDough";
import { PizzaSauceProgram } from "./node.pizzaSauce";
import type { IconAtlas } from "./iconAtlas";
import type { StructuralDiagramMode } from "types";
import { PizzaToppingProgram } from "./node.pizzaTopping";

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
11 [x] fries
12 [x] chicken
13 [ ] spinach
14 [x] mushrooms
15 [x] olomoucké syrečky
16 [ ] dried tomatoes
17 [ ] onion
18 [ ] paprika
19 [x] shrimp
20 [x] olives
21 [x] pickle
22 [ ] tuna
23 [ ] pepperoni
24 [ ] gouda
25 [x] eidam
26 [x] cookie
27 [x] meatball
*/
const { UNSIGNED_BYTE, FLOAT } = WebGLRenderingContext;

const UNIFORMS = ["u_sizeRatio", "u_pixelRatio", "u_matrix", "u_crustWidth", "u_sauceWidth"];

export interface PizzaProgramOptions {
    isPizzaEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
    iconAtlas: IconAtlas;
    diagramMode: StructuralDiagramMode;
}

export const DEFAULT_PIZZA_PROGRAM_OPTIONS = {
    isPizzaEnabled: false,
    crustWidth: 20,
    sauceWidth: 40
};

export default function createPizzaProgram(options: PizzaProgramOptions): NodeProgramConstructor {

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
        class extends PizzaToppingProgram {
            constructor(gl: WebGLRenderingContext, renderer: Sigma) {
                super(gl, renderer, options as PizzaProgramOptions);
            }
        }
    ]);
}
