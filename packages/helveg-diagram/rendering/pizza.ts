import { Attributes } from "graphology-types";
import { Sigma, AbstractNodeProgram, NodeDisplayData, RenderParams, getPixelRatio, NodeHoverDrawingFunction, NodeLabelDrawingFunction } from "../deps/sigma.ts";
import { HelvegNodeProgramType } from "../diagram/initializers.ts";
import { ILogger } from "../model/logger.ts";
import { EMPTY_ICON_ATLAS, IconAtlas } from "./iconAtlas.ts";
import { PizzaDoughProgram } from "./node.pizzaDough.ts";
import { PizzaSauceProgram } from "./node.pizzaSauce.ts";
import { PizzaToppingProgram } from "./node.pizzaTopping.ts";

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

export interface PizzaProgramOptions {
    isPizzaEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
    iconAtlas: Readonly<IconAtlas>;
    showOnlyHighlighted: boolean;
    pizzaToppings: Record<string, string>;
}

export const DEFAULT_PIZZA_PROGRAM_OPTIONS: PizzaProgramOptions = {
    isPizzaEnabled: false,
    crustWidth: 20,
    sauceWidth: 40,
    iconAtlas: EMPTY_ICON_ATLAS,
    showOnlyHighlighted: false,
    pizzaToppings: {}
};

export default function createPizzaProgram(options: PizzaProgramOptions): HelvegNodeProgramType {

    return class extends PizzaProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {
            super(gl, pickingBuffer, renderer, options);
        }
    };
}

export class PizzaProgram implements AbstractNodeProgram {
    doughProgram: AbstractNodeProgram;
    sauceProgram: AbstractNodeProgram;
    toppingProgram: AbstractNodeProgram;
    doughCanvas: HTMLCanvasElement;
    doughContext: WebGL2RenderingContext;

    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        private sigma: Sigma,
        private options: PizzaProgramOptions,
        private logger?: ILogger) {

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

        this.doughProgram = new PizzaDoughProgram(this.doughContext, pickingBuffer, sigma, options);
        this.sauceProgram = new PizzaSauceProgram(this.doughContext, pickingBuffer, sigma, options);
        // NB: toppings are rendered in the regular `nodes` layer
        this.toppingProgram = new PizzaToppingProgram(gl, pickingBuffer, sigma, options);
    }

    process(nodeIndex: number, offset: number, data: NodeDisplayData): void {
        this.doughProgram.process(nodeIndex, offset, data);
        this.sauceProgram.process(nodeIndex, offset, data);
        this.toppingProgram.process(nodeIndex, offset, data);
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

    drawLabel: NodeLabelDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;
    drawHover: NodeHoverDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;

    kill(): void {
        this.doughProgram.kill();
        this.sauceProgram.kill();
        this.toppingProgram.kill();
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
        this.logger?.debug("Destroying a CodePizza canvas");

        this.doughCanvas.remove();
        this.doughCanvas = null!;
        this.sigma = null!;
    }
}
