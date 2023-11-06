import { NodeProgramConstructor, AbstractNodeProgram, Sigma, NodeDisplayData, RenderParams } from "../deps/sigma.ts";
import { SigmaEffectsExtension } from "./effects.ts";
import { FireProgramOptions } from "./node.fire.ts";
import createIconProgram, { IconProgramOptions } from "./node.icon.ts";
import createOutlinesProgram, { OutlinesProgramOptions } from "./node.outlines.ts";
import createPizzaProgram, { PizzaProgramOptions } from "./pizza.ts";

export interface GlyphProgramOptions
    extends IconProgramOptions, OutlinesProgramOptions, FireProgramOptions, PizzaProgramOptions {

    showIcons: boolean;
    showOutlines: boolean;
    showFire: boolean;
}

export function createGlyphProgram(options: GlyphProgramOptions): NodeProgramConstructor {
    return class extends AbstractNodeProgram {
        private effects: SigmaEffectsExtension;
        private iconProgram: AbstractNodeProgram;
        private outlinesProgram: AbstractNodeProgram;
        private effectsProgram: AbstractNodeProgram;
        private pizzaProgram: AbstractNodeProgram;

        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer);
            
            
            // NB: The effects extension's lifetime is as long as of this program.
            //     This is done on purpose since Sigma creates one instance of each program for "regular" nodes
            //     and one for "hovered" nodes. A separate effects canvas must exist for each kind.
            this.effects = new SigmaEffectsExtension(options);

            this.iconProgram = new (createIconProgram(options))(gl, renderer);
            this.outlinesProgram = new (createOutlinesProgram(options))(gl, renderer);
            this.effectsProgram = new this.effects.program(gl, renderer);
            
            // NB: Pizza needs to be initialized after the effects program so that its canvas is below effects.
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
            // NB: let pizza program handle this on its own since it manages its own canvas
            this.pizzaProgram.render(params);

            // NB: let the effects extension handle this on its own since it manages its own canvas
            this.effectsProgram.render(params);
            if (!options.isPizzaEnabled) {
                if (options.showOutlines) {
                    this.outlinesProgram.render(params);
                }
                
                if (options.showIcons) {
                    this.iconProgram.render(params);
                }
            }

        }
    };
}
