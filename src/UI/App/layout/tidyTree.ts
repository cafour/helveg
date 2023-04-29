import type Graph from "graphology";
import { tree, hierarchy } from "d3-hierarchy";
import type { Coordinates } from "model/graph";

export interface TidyTreeLayoutOptions {
    radius: number;
    relation: string | null;
    offset: Coordinates
}

const DEFAULT_OPTIONS: TidyTreeLayoutOptions = {
    radius: 100,
    relation: "declares",
    offset: {x: 0, y: 0}
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
            graph.setNodeAttribute(node.data, "x", opts.offset.x + y * Math.cos(x));
            graph.setNodeAttribute(node.data, "y", opts.offset.y + y * Math.sin(x));
        }
    })
}
