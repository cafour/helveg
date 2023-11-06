import Graph, { Attributes } from "../deps/graphology.ts";

export interface BfsOptions {
    maxDepth: number;
    relation: string | null;
    callback: (node: string, attr: Attributes, depth: number) => boolean | void;
}

interface BfsItem {
    node: string;
    depth: number;
}

export function bfs(graph: Graph, nodeId: string, options?: Partial<BfsOptions>): Set<string> {
    let opts = { maxDepth: Number.MAX_SAFE_INTEGER, ...options };

    let visited = new Set<string>();
    let queue: BfsItem[] = [{ node: nodeId, depth: 0 }];
    while (queue.length > 0) {
        let { node, depth } = queue.shift()!;
        if (visited.has(node) || !graph.hasNode(node) || depth > opts.maxDepth) {
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
                if (depth + 1 > opts.maxDepth) {
                    return;
                }

                queue.push({ node: dst, depth: depth + 1});
            });
        }
    }
    return visited;
}
