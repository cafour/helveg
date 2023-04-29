import type Graph from "graphology";
import type { Attributes } from "graphology-types";

export interface BfsOptions {
    maxDepth: number;
    relation: string | null;
    callback: (node: string, attr: Attributes, depth: number) => boolean | void;
}

export function bfs(graph: Graph, nodeId: string, options?: Partial<BfsOptions>): Set<string> {
    let opts = { ...options };

    let visited = new Set<string>();
    let queue = [nodeId];
    let depth = 0;
    while (queue.length > 0) {
        let node = queue.shift()!;
        if (visited.has(node) || !graph.hasNode(node)) {
            continue;
        }
        visited.add(node);

        let visitChildren = true;
        if (opts.callback) {
            let result = opts.callback(node, graph.getNodeAttributes(node), depth);
            visitChildren = result === true ? true : result === false ? false : visitChildren;
        }

        if (visitChildren) {
            graph.forEachOutboundEdge(node, (edge, attr, src, dst, srcAttr, dstAttr, undir) => {
                if (opts.relation && attr.relation !== opts.relation) {
                    return;
                }
                queue.push(dst);
            });
        }
    }
    return visited;
}
