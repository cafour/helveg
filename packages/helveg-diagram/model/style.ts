import { MultigraphEdge, MultigraphNode } from "./data-model.ts";
import { HelvegNodeAttributes } from "./graph.ts";

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

export interface Slices {
    stroked: number,
    solid: number,
    width: number,
}

export type Outlines = []
    | [Outline]
    | [Outline, Outline]
    | [Outline, Outline, Outline]
    | [Outline, Outline, Outline, Outline];

export enum Contour {
    None = 0.0,
    FullOctagon = 1.0,
    DashedHexagon = 2.0,
};

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
    backgroundColor?: string;
    size: number;
    outlines: Outlines;
    slices: Slices;
    fire: FireStatus;
    contour: Contour;
}

export const FALLBACK_NODE_ICON = "vscode:pie-chart";

export const FALLBACK_NODE_STYLE: NodeStyle = {
    size: 5,
    color: "#202020",
    icon: FALLBACK_NODE_ICON,
    outlines: [],
    slices: { stroked: 0, solid: 1, width: 0 },
    fire: FireStatus.None,
    contour: Contour.None,
};

export interface EdgeStyle {
    color: string;
    width: number;
    type: string;
}

export const FALLBACK_EDGE_STYLE: EdgeStyle = {
    color: "#202020",
    width: 1,
    type: "line"
};

export type NodeStylist = (node: HelvegNodeAttributes) => NodeStyle;
export type RelationStylist = (relation: string) => EdgeStyle;
export type EdgeStylist = (relation: string, edge: MultigraphEdge) => EdgeStyle;

export function fallbackNodeStylist(_node: MultigraphNode): NodeStyle {
    return FALLBACK_NODE_STYLE;
}

export function fallbackRelationStylist(_relation: string) {
    return FALLBACK_EDGE_STYLE;
}

export function fallbackEdgeStylist(_relation: string, _edge: MultigraphEdge) {
    return FALLBACK_EDGE_STYLE;
}
