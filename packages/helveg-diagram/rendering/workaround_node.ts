/**
 * This file duplicated Sigma's Program and NodeProgram classes to work around an issue preventing use of GLSL 3.0 ES.
 * 
 * https://github.com/jacomyal/sigma.js/issues/1499
 */
import { Attributes } from "graphology-types";

import {
    Sigma,
    NodeDisplayData, NonEmptyArray, RenderParams,
    indexToColor,
    NodeHoverDrawingFunction,
    NodeLabelDrawingFunction,
    AbstractProgram
} from "../deps/sigma.ts";

import { WorkaroundProgram as Program } from "./workaround_program.ts";

export abstract class AbstractNodeProgram<
    N extends Attributes = Attributes,
    E extends Attributes = Attributes,
    G extends Attributes = Attributes,
> extends AbstractProgram<N, E, G> {
    abstract drawLabel: NodeLabelDrawingFunction<N, E, G> | undefined;
    abstract drawHover: NodeHoverDrawingFunction<N, E, G> | undefined;
    abstract process(nodeIndex: number, offset: number, data: NodeDisplayData): void;
}

export abstract class WorkaroundNodeProgram<
    Uniform extends string = string,
    N extends NodeDisplayData = NodeDisplayData,
    E extends Attributes = Attributes,
    G extends Attributes = Attributes,
>
    extends Program<Uniform, N, E, G>
    implements AbstractNodeProgram<N, E, G> {
    drawLabel: NodeLabelDrawingFunction<N, E, G> | undefined;
    drawHover: NodeHoverDrawingFunction<N, E, G> | undefined;

    kill(): void {
        super.kill();
    }

    process(nodeIndex: number, offset: number, data: N): void {
        let i = offset * this.STRIDE;
        // NOTE: dealing with hidden items automatically
        if (data.hidden) {
            for (let l = i + this.STRIDE; i < l; i++) {
                this.array[i] = 0;
            }
            return;
        }

        return this.processVisibleItem(indexToColor(nodeIndex), i, data);
    }

    abstract processVisibleItem(nodeIndex: number, i: number, data: NodeDisplayData): void;
}
