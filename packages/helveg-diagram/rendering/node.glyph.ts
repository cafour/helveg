import { Attributes } from "graphology-types";
import { AbstractNodeProgram, Sigma, NodeDisplayData, RenderParams, NodeHoverDrawingFunction, NodeLabelDrawingFunction } from "../deps/sigma.ts";
import { HelvegNodeProgramType } from "../diagram/initializers.ts";
import { ILogger, sublogger } from "../model/logger.ts";
import { SigmaEffectsExtension } from "./effects.ts";
import createDiffProgram, { DEFAULT_DIFF_PROGRAM_OPTIONS, DiffProgramOptions } from "./node.diff.ts";
import { DEFAULT_FIRE_PROGRAM_OPTIONS, FireProgramOptions } from "./node.fire.ts";
import createIconProgram, { DEFAULT_ICON_PROGRAM_OPTIONS, IconProgramOptions } from "./node.icon.ts";
import createOutlinesProgram, { DEFAULT_OUTLINES_PROGRAM_OPTIONS, OutlinesProgramOptions } from "./node.outlines.ts";
import createPizzaProgram, { DEFAULT_PIZZA_PROGRAM_OPTIONS, PizzaProgramOptions } from "./pizza.ts";

export type SizingMode = "linear" | "sqrt" | "log";

export interface GlyphProgramOptions
    extends IconProgramOptions, OutlinesProgramOptions, FireProgramOptions, PizzaProgramOptions, DiffProgramOptions {

    showIcons: boolean;
    showOutlines: boolean;
    showFire: boolean;
    showLabels: boolean;
    showDiffs: boolean;
    sizingMode: SizingMode;
}

export const DEFAULT_GLYPH_PROGRAM_OPTIONS: GlyphProgramOptions =
{
    showIcons: true,
    showOutlines: true,
    showFire: true,
    showLabels: false,
    showDiffs: true,
    sizingMode: "linear",
    ...DEFAULT_ICON_PROGRAM_OPTIONS,
    ...DEFAULT_OUTLINES_PROGRAM_OPTIONS,
    ...DEFAULT_FIRE_PROGRAM_OPTIONS,
    ...DEFAULT_PIZZA_PROGRAM_OPTIONS,
    ...DEFAULT_DIFF_PROGRAM_OPTIONS
};

export function createGlyphProgram(options: GlyphProgramOptions, logger?: ILogger): HelvegNodeProgramType {
    return class implements AbstractNodeProgram {
        private effects: SigmaEffectsExtension;
        private iconProgram: AbstractNodeProgram;
        private outlinesProgram: AbstractNodeProgram;
        private effectsProgram: AbstractNodeProgram;
        private pizzaProgram: AbstractNodeProgram;
        private diffProgram: AbstractNodeProgram;

        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: Sigma) {

            // NB: The effects extension's lifetime is as long as of this program.
            //     This is done on purpose since Sigma creates one instance of each program for "regular" nodes
            //     and one for "hovered" nodes. A separate effects canvas must exist for each kind.
            this.effects = new SigmaEffectsExtension(options, logger ? sublogger(logger, "effects") : undefined);

            this.iconProgram = new (createIconProgram(options))(gl, pickingBuffer, renderer);
            this.outlinesProgram = new (createOutlinesProgram(options))(gl, pickingBuffer, renderer);
            this.effectsProgram = new this.effects.program(gl, pickingBuffer, renderer);

            // NB: Pizza needs to be initialized after the effects program so that its canvas is below effects.
            this.pizzaProgram = new (createPizzaProgram(options))(gl, pickingBuffer, renderer);

            this.diffProgram = new (createDiffProgram(options))(gl, pickingBuffer, renderer);
        }

        process(nodeIndex: number, offset: number, data: NodeDisplayData): void {
            this.outlinesProgram.process(nodeIndex, offset, data);
            this.iconProgram.process(nodeIndex, offset, data);
            this.effectsProgram.process(nodeIndex, offset, data);
            this.pizzaProgram.process(nodeIndex, offset, data);
            this.diffProgram.process(nodeIndex, offset, data);
        }

        reallocate(capacity: number): void {
            this.outlinesProgram.reallocate(capacity);
            this.iconProgram.reallocate(capacity);
            this.effectsProgram.reallocate(capacity);
            this.pizzaProgram.reallocate(capacity);
            this.diffProgram.reallocate(capacity);
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

                if (options.showDiffs) {
                    this.diffProgram.render(params);
                }
            }
        }

        drawLabel: NodeLabelDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;
        drawHover: NodeHoverDrawingFunction<Attributes, Attributes, Attributes> | undefined = undefined;

        kill(): void {
            this.outlinesProgram.kill();
            this.iconProgram.kill();
            this.effectsProgram.kill();
            this.pizzaProgram.kill();
            this.diffProgram.kill();
        }
    };
}
