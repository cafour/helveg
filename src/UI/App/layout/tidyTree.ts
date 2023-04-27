import type Graph from "graphology";
import { tree, hierarchy } from "d3-hierarchy";

export interface TidyTreeLayoutOptions {
    radius: number;
    relation: string;
}

const DEFAULT_OPTIONS: TidyTreeLayoutOptions = {
    radius: 100,
    relation: "declares"
}

export default function tidyTree(graph: Graph, rootId: string, options?: Partial<TidyTreeLayoutOptions>) {
    let opts = { ...DEFAULT_OPTIONS, ...options };
    const root = hierarchy(rootId, nodeId => 
        <string[]>graph.mapOutboundEdges(nodeId, (_, attr, __, dst) => attr.relation && attr.relation == opts.relation
            ? dst
            : undefined)
            .filter(dst => dst != undefined));
    tree<string>()
        .size([2 * Math.PI, opts.radius])
        .separation((a, b) => (a.parent == b.parent ? 1 : 2) / a.depth)
        (root);

    root.each(node => {
        const { x, y } = <typeof node & { x: number, y: number }>node;
        if (x != null && y != null && !isNaN(x) && !isNaN(y)) {
            graph.setNodeAttribute(node.data, "x", y * Math.cos(x));
            graph.setNodeAttribute(node.data, "y", y * Math.sin(x));
        }
    })
}
