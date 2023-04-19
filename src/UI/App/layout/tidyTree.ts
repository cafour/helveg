import type Graph from "graphology";
import { tree, hierarchy } from "d3-hierarchy";

// export interface TidyTreeLayoutOptions {
//     scale: number;
    
// }

// const defaultOptions: TidyTreeLayoutOptions;

export default function tidyTree(graph: Graph, rootId: string, radius: number) {
    const root = hierarchy(rootId, nodeId => graph.outNeighbors(nodeId));
    tree<string>()
        .size([2 * Math.PI, radius])
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
