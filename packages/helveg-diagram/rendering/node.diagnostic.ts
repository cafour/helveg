import { floatColor } from "../deps/sigma.ts";
import { HelvegNodeAttributes, HelvegNodeProgramType, HelvegSigma } from "../model/graph.ts";
import { DiagnosticIndicatorStyle, FALLBACK_NODE_ICON, FALLBACK_NODE_STYLE } from "../model/style.ts";
import { IconAtlasEntryStatus } from "./iconAtlas.ts";
import { IconProgram, IconProgramOptions } from "./node.icon";

export interface DiagnosticProgramOptions extends IconProgramOptions {
}

export const DEFAULT_DIAGNOSTIC_PROGRAM_OPTIONS: DiagnosticProgramOptions = {
    iconAtlas: null!,
    iconCorner: { x: 0, y: 0 },
    showOnlyHighlighted: false,
};

export default function createDiagnosticProgram(options: DiagnosticProgramOptions): HelvegNodeProgramType {
    return class extends DiagnosticProgram {
        constructor(gl: WebGLRenderingContext, pickingBuffer: WebGLFramebuffer, renderer: HelvegSigma) {
            super(gl, pickingBuffer, renderer, options);
        }
    };
}

export class DiagnosticProgram extends IconProgram {
    constructor(
        gl: WebGLRenderingContext,
        pickingBuffer: WebGLFramebuffer,
        renderer: HelvegSigma,
        private diagnosticOptions: DiagnosticProgramOptions
    ) {
        super(gl, pickingBuffer, renderer, diagnosticOptions);
        diagnosticOptions.iconAtlas.tryAddIcon("helveg:DiagnosticWarning");
        diagnosticOptions.iconAtlas.tryAddIcon("helveg:DiagnosticError");
    }

    processVisibleItem(nodeIndex: number, offset: number, data: HelvegNodeAttributes): void {
        const array = this.array;
        this.diagnosticOptions.iconAtlas.tryAddIcon(data.icon ?? FALLBACK_NODE_ICON);

        const useColor = !this.diagnosticOptions.showOnlyHighlighted || data.highlighted === true;

        const indicatorSize = (data.baseSize ?? 2) * 0.25;
        const params = this.renderer.getRenderParams();
        const posOffset =
            ((data.size + indicatorSize * 1.9) * params.correctionRatio) /
            params.sizeRatio;

        array[offset++] = (data.x ?? 0) - posOffset;
        array[offset++] = (data.y ?? 0) + posOffset;
        array[offset++] = indicatorSize;
        array[offset++] = floatColor(useColor ? data.color ?? FALLBACK_NODE_STYLE.color : "#aaaaaa");

        let icon =
            data.diagnosticIndicator === DiagnosticIndicatorStyle.ERROR
                ? "helveg:DiagnosticError"
                : data.diagnosticIndicator === DiagnosticIndicatorStyle.WARNING
                ? "helveg:DiagnosticWarnining"
                : null;

        let atlasEntry = icon ? this.diagnosticOptions.iconAtlas.entries[icon] : null;
        if (atlasEntry && atlasEntry.status === IconAtlasEntryStatus.Rendered) {
            array[offset++] = atlasEntry.x / this.diagnosticOptions.iconAtlas.width;
            array[offset++] = atlasEntry.y / this.diagnosticOptions.iconAtlas.height;
            array[offset++] = this.diagnosticOptions.iconAtlas.iconSize / this.diagnosticOptions.iconAtlas.width;
            array[offset++] = this.diagnosticOptions.iconAtlas.iconSize / this.diagnosticOptions.iconAtlas.height;
        } else {
            // the icon is not ready yet, so don't render it
            array[offset++] = 0;
            array[offset++] = 0;
            array[offset++] = 0;
            array[offset++] = 0;
        }
    }
}
