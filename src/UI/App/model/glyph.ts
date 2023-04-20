import type { GraphNode } from "./multigraph";

let INT8 = new Int8Array(4);
let INT32 = new Int32Array(INT8.buffer);
let FLOAT32 = new Float32Array(INT8.buffer);

export enum OutlineStyle {
    Solid = 0,
    Dashed = 1
}

export interface Outline {
    style: OutlineStyle;
    width: number;
}

export type Outlines = [] | [Outline] | [Outline, Outline] | [Outline, Outline, Outline];

export function floatOutlines(outlines: Outlines): number {
    if (!outlines) {
        return 0;
    }
    
    let l0 = (outlines[0]?.width ?? 0) & 0x3f | ((outlines[0]?.style ?? 0) & 0x3) << 6;
    let l1 = (outlines[1]?.width ?? 0) & 0x3f | ((outlines[1]?.style ?? 0) & 0x3) << 6;;
    let l2 = (outlines[2]?.width ?? 0) & 0x3f | ((outlines[2]?.style ?? 0) & 0x3) << 6;;
    INT32[0] = (l0 | l1 << 8 | l2 << 16) & 0xffffff;
    return FLOAT32[0];
}

export interface NodeStyle {
    icon: string;
    color: string;
    size: number;
    outlines: Outlines;
}

export interface GlyphStyle {
    apply(node: GraphNode): NodeStyle;
}

export class StaticGlyphStyle implements GlyphStyle {
    constructor(public style: NodeStyle) {
    }

    apply(_node: GraphNode) {
        return this.style;
    }
}
