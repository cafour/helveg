import { AbstractNodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import { IconProgram, type IconProgramOptions } from "./node.icon";
import type { Sigma } from "sigma";
import { OutlinesProgram, type OutlinesProgramOptions } from "./node.outlines";
import type { FireProgramOptions } from "./node.fire";
import { SigmaEffectsExtension } from "./effects";
import type { PizzaProgramOptions } from "./pizza";
import createPizzaProgram from "./pizza";

export interface GlyphProgramOptions
    extends IconProgramOptions, OutlinesProgramOptions, FireProgramOptions, PizzaProgramOptions {

    showIcons: boolean;
    showOutlines: boolean;
    showFire: boolean;
}

export function createGlyphProgram(options: GlyphProgramOptions): NodeProgramConstructor {
    return class extends AbstractNodeProgram {
        private effects: SigmaEffectsExtension;
        private iconProgram: IconProgram;
        private outlinesProgram: OutlinesProgram;
        private effectsProgram: AbstractNodeProgram;
        private pizzaProgram: AbstractNodeProgram;

        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer);
            
            // NB: The effects extension's lifetime is as long as of this program.
            //     This is done on purpose since Sigma creates one instance of each program for "regular" nodes
            //     and one for "hovered" nodes. A separate effects canvas must exist for each kind.
            this.effects = new SigmaEffectsExtension(options);
            
            this.iconProgram = new IconProgram(gl, renderer, options);
            this.outlinesProgram = new OutlinesProgram(gl, renderer, options);
            this.effectsProgram = new this.effects.program(gl, renderer);
            this.pizzaProgram = new (createPizzaProgram(options))(gl, renderer);
        }

        process(offset: number, data: NodeDisplayData): void {
            this.outlinesProgram.process(offset, data);
            this.iconProgram.process(offset, data);
            this.effectsProgram.process(offset, data);
            this.pizzaProgram.process(offset, data);
        }

        reallocate(capacity: number): void {
            this.outlinesProgram.reallocate(capacity);
            this.iconProgram.reallocate(capacity);
            this.effectsProgram.reallocate(capacity);
            this.pizzaProgram.reallocate(capacity);
        }

        render(params: RenderParams): void {
            if (options.isPizzaEnabled) {
                this.pizzaProgram.render(params);
            }
            
            if (options.showOutlines) {
                this.outlinesProgram.render(params);
            }

            if (options.showIcons) {
                this.iconProgram.render(params);
            }

            // let the effects extension handle this on its own since it manages its own canvas
            this.effectsProgram.render(params);
        }
    };
}
