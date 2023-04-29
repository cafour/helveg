import type { Node } from "./multigraph";

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

export type Outlines = []
    | [Outline]
    | [Outline, Outline]
    | [Outline, Outline, Outline]
    | [Outline, Outline, Outline, Outline];

export function floatOutlineWidths(outlines: Outlines): number {
    if (!outlines) {
        return 0;
    }

    let w0 = outlines[0]?.width ?? 0;
    let w1 = outlines[1]?.width ?? 0;
    let w2 = outlines[2]?.width ?? 0;
    let w3 = outlines[3]?.width ?? 0;
    let totalWidth = w0 + w1 + w2 + w3;

    let l0 = (w0 / totalWidth * 255) & 0xff;
    let l1 = (w1 / totalWidth * 255) & 0xff;
    let l2 = (w2 / totalWidth * 255) & 0xff;
    let l3 = (w3 / totalWidth * 255) & 0xff;
    INT32[0] = (l0 | l1 << 8 | l2 << 16 | l3 << 24) & 0xffffffff;
    return FLOAT32[0];
}

export function floatOutlineStyles(outlines: Outlines): number {
    if (!outlines) {
        return 0;
    }

    let s0 = outlines[0]?.style ?? OutlineStyle.Solid;
    let s1 = outlines[1]?.style ?? OutlineStyle.Solid;
    let s2 = outlines[2]?.style ?? OutlineStyle.Solid;
    let s3 = outlines[3]?.style ?? OutlineStyle.Solid;
    INT32[0] = (s0 | s1 << 8 | s2 << 16 | s3 << 24) & 0xffffffff;
    return FLOAT32[0];
}

export function getOutlinesTotalWidth(outlines: Outlines): number {
    if (!outlines) {
        return 0;
    }

    let w0 = outlines[0]?.width ?? 0;
    let w1 = outlines[1]?.width ?? 0;
    let w2 = outlines[2]?.width ?? 0;
    let w3 = outlines[3]?.width ?? 0;
    return w0 + w1 + w2 + w3;
}

export enum FireStatus {
    None = "none",
    Smoke = "smoke",
    Flame = "flame"
}

export interface NodeStyle {
    icon: string;
    color: string;
    size: number;
    outlines: Outlines;
    fire: FireStatus;
}

export interface GlyphStyle {
    name: string;
    apply(node: Node): NodeStyle;
}

export class StaticGlyphStyle implements GlyphStyle {
    name: string = "Fallback";

    constructor(public style: NodeStyle) {
    }

    apply(_node: Node) {
        return this.style;
    }
}

export const FALLBACK_ICON_NAME = "base:PolarChart";

export const FALLBACK_STYLE = new StaticGlyphStyle({
    size: 5,
    color: "#202020",
    icon: FALLBACK_ICON_NAME,
    outlines: [],
    fire: FireStatus.None
});

export class GlyphStyleRegistry {
    private styles = new Map<string, GlyphStyle>();

    register(namespace: string | null, style: GlyphStyle): void {
        let name = namespace && !style.name.startsWith(`${namespace}:`) ? `${namespace}:${style.name}` : style.name;
        if (this.styles.has(name)) {
            throw new Error(`The registry already contains a style named '${style.name}'.`);
        }

        this.styles[name] = style;
    }

    get(styleName: string): GlyphStyle {
        let style = this.styles[styleName];
        if (!style) {
            DEBUG && console.log(`Could not find the '${styleName}' style.`);
            return FALLBACK_STYLE;
        }

        return style;
    }
}
