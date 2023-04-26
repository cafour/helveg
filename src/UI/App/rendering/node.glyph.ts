import { AbstractNodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { IconAtlas } from "./iconAtlas";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import { IconProgram, type IconProgramOptions } from "./node.icon";
import type { Sigma } from "sigma";
import { OutlinesProgram, type OutlinesProgramOptions } from "./node.outlines";
import type { StructuralDiagramMode } from "model/structural";
import { FireProgram, type FireProgramOptions } from "./node.fire";

export interface GlyphProgramOptions
    extends IconProgramOptions, OutlinesProgramOptions, FireProgramOptions {

    showIcons: boolean;
    showOutlines: boolean;
    showFire: boolean;
}

export function createGlyphProgram(options: GlyphProgramOptions): NodeProgramConstructor {
    return class extends AbstractNodeProgram {
        private iconProgram: IconProgram;
        private outlinesProgram: OutlinesProgram;
        private fireProgram: FireProgram;

        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer);
            this.iconProgram = new IconProgram(gl, renderer, options);
            this.outlinesProgram = new OutlinesProgram(gl, renderer, options);
            this.fireProgram = new FireProgram(gl, renderer, options);
        }

        process(offset: number, data: NodeDisplayData): void {
            this.outlinesProgram.process(offset, data);
            this.iconProgram.process(offset, data);
            this.fireProgram.process(offset, data);
        }

        reallocate(capacity: number): void {
            this.outlinesProgram.reallocate(capacity);
            this.iconProgram.reallocate(capacity);
            this.fireProgram.reallocate(capacity);
        }

        render(params: RenderParams): void {
            if (options.showOutlines) {
                this.outlinesProgram.render(params);
            }
            if (options.showIcons) {
                this.iconProgram.render(params);
            }
            if(options.showFire) {
                this.fireProgram.render(params);
            }
        }
    };
}
