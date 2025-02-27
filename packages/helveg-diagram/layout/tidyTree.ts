import { hierarchy, tree } from "../deps/d3-hierarchy.ts";
import { Coordinates } from "../deps/sigma.ts";
import { HelvegGraph } from "../model/graph.ts";

export interface TidyTreeLayoutOptions {
    radius: number;
    levelWidth?: number;
    relation: string | null;
    offset: Coordinates
}

const DEFAULT_OPTIONS: TidyTreeLayoutOptions = {
    radius: 100,
    relation: null,
    offset: { x: 0, y: 0 }
}

export default function tidyTree(graph: HelvegGraph, rootId: string, options?: Partial<TidyTreeLayoutOptions>) {
    let opts = { ...DEFAULT_OPTIONS, ...options };
    const root = hierarchy(rootId, nodeId =>
        <string[]>graph.mapOutboundEdges(nodeId, (_edge, attr, _src, dst, _srcAttr, dstAttr) =>
            attr.relation && attr.relation === opts.relation
                ? dst
                : undefined)
            .filter(dst => dst != undefined));
    const radius = opts.levelWidth != null ? root.depth * opts.levelWidth : opts.radius;
    tree<string>()
        .size([2 * Math.PI, radius])
        .separation((a, b) => (a.parent == b.parent ? 1 : 2) / a.depth)
        (root);

    root.each(node => {
        const { x, y } = <typeof node & { x: number, y: number }>node;
        if (x != null && y != null && !isNaN(x) && !isNaN(y)) {
            graph.setNodeAttribute(node.data, "x", opts.offset.x + y * Math.cos(x));
            graph.setNodeAttribute(node.data, "y", opts.offset.y + y * Math.sin(x));
            graph.setNodeAttribute(node.data, "inInitialPosition", true);
            graph.setNodeAttribute(node.data, "childCount", node.children?.length ?? 0);
            graph.setNodeAttribute(node.data, "descendantCount", node.descendants().length);
        }
    })
}
