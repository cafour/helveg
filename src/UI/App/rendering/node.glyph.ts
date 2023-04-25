import { AbstractNodeProgram, type NodeProgramConstructor } from "sigma/rendering/webgl/programs/common/node";
import type { IconAtlas } from "./iconAtlas";
import type { NodeDisplayData, RenderParams } from "sigma/types";
import { IconProgram } from "./node.icon";
import type { Sigma } from "sigma";
import { OutlinesProgram } from "./node.outlines";
import type { StructuralDiagramMode } from "model/structural";

export interface GlyphProgramOptions {
    showIcons: boolean;
    showOutlines: boolean;
    gap: number;
    iconAtlas: IconAtlas;
    diagramMode: StructuralDiagramMode;
}

export function createGlyphProgram(options: GlyphProgramOptions): NodeProgramConstructor {
    return class extends AbstractNodeProgram {
        private iconProgram: IconProgram;
        private outlinesProgram: OutlinesProgram;

        constructor(gl: WebGLRenderingContext, renderer: Sigma) {
            super(gl, renderer);
            this.iconProgram = new IconProgram(gl, renderer, options);
            this.outlinesProgram = new OutlinesProgram(gl, renderer, options);
        }

        process(offset: number, data: NodeDisplayData): void {
            this.outlinesProgram.process(offset, data);
            this.iconProgram.process(offset, data);
        }

        reallocate(capacity: number): void {
            this.outlinesProgram.reallocate(capacity);
            this.iconProgram.reallocate(capacity);
        }

        render(params: RenderParams): void {
            if (options.showOutlines) {
                this.outlinesProgram.render(params);
            }
            if (options.showIcons) {
                this.iconProgram.render(params);
            }
        }
    };
}
