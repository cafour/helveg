import type Graph from "graphology";
import type { FireStatus, Outlines } from "./glyph";
import type { NodeDisplayData } from "sigma/types";

export interface HelvegNodeAttributes extends NodeDisplayData {
    style: string;
    icon: string;
    iconSize: number;
    outlines: Outlines;
    fire: FireStatus;
    fixed: boolean;
}

export type HelvegGraph = Graph<Partial<HelvegNodeAttributes>>;
