import { AbstractNodeProgram, createNodeCompoundProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type Sigma from "sigma";
import { PizzaDoughProgram } from "./node.pizzaDough";
import { PizzaSauceProgram } from "./node.pizzaSauce";
import type { IconAtlas } from "./iconAtlas";
import type { StructuralDiagramMode } from "types";
import { PizzaToppingProgram } from "./node.pizzaTopping";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import { getPixelRatio } from "sigma/utils";

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

    return class extends PizzaProgram {
        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer, options);
        }
    };
}

export class PizzaProgram extends AbstractNodeProgram {
    doughProgram: AbstractNodeProgram;
    sauceProgram: AbstractNodeProgram;
    toppingProgram: AbstractNodeProgram;
    doughCanvas: HTMLCanvasElement;
    doughContext: WebGL2RenderingContext;

    constructor(gl: WebGLRenderingContext, private sigma: Sigma, private options: PizzaProgramOptions) {
        super(gl, sigma);

        this.doughCanvas = document.createElement("canvas");
        this.doughCanvas.classList.add("helveg-codepizza");
        this.doughCanvas.style.position = "absolute";

        sigma.getContainer().prepend(this.doughCanvas);
        this.doughContext = this.doughCanvas.getContext("webgl2", {
            preserveDrawingBuffer: false,
            antialias: false
        })!;
        if (!this.doughContext) {
            throw new Error("WebGL2 is not supported");
        }
        sigma.addListener("kill", this.onSigmaKill.bind(this));
        sigma.addListener("resize", this.onSigmaResize.bind(this));

        this.doughProgram = new PizzaDoughProgram(this.doughContext, sigma, options);
        this.sauceProgram = new PizzaSauceProgram(this.doughContext, sigma, options);
        // NB: toppings are rendered in the regular `nodes` layer
        this.toppingProgram = new PizzaToppingProgram(gl, sigma, options);
    }

    process(offset: number, data: NodeDisplayData): void {
        this.doughProgram.process(offset, data);
        this.sauceProgram.process(offset, data);
        this.toppingProgram.process(offset, data);
    }

    reallocate(capacity: number): void {
        this.doughProgram.reallocate(capacity);
        this.sauceProgram.reallocate(capacity);
        this.toppingProgram.reallocate(capacity);
    }

    render(params: RenderParams): void {
        this.doughContext.clear(this.doughContext.COLOR_BUFFER_BIT);
        if (!this.options.isPizzaEnabled) {
            return;
        }

        this.doughProgram.render(params);
        this.sauceProgram.render(params);
        this.toppingProgram.render(params);
    }
    
    private onSigmaResize() {
        let width = this.sigma.getContainer().offsetWidth;
        let height = this.sigma.getContainer().offsetHeight;
        let pixelRatio = getPixelRatio();

        this.doughCanvas.setAttribute("width", width * pixelRatio + "px");
        this.doughCanvas.setAttribute("height", height * pixelRatio + "px");
        this.doughCanvas.style.width = width + "px";
        this.doughCanvas.style.height = height + "px";
        this.doughContext.viewport(0, 0, width * pixelRatio, height * pixelRatio);
    }

    private onSigmaKill(): void {
        DEBUG && console.log("Destroying a CodePizza canvas");

        this.doughCanvas.remove();
        this.doughCanvas = null!;
        this.sigma = null!;
    }
}
