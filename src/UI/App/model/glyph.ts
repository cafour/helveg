import type { GraphNode } from "./multigraph";

export enum LineStyle {
    Solid,
    Dashed,
    Dotted
}

export class Outline {
    constructor(
        public style: LineStyle = LineStyle.Solid,
        public color: string = "#000000",
        public width: number = 1) {
    }
}

export interface NodeStyle {
    icon: string;
    color: string;
    size: number;
    outlines: Outline[];
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
