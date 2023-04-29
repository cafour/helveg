import type Graph from "graphology";
import type { FireStatus, Outlines } from "./glyph";
import type { EdgeDisplayData, NodeDisplayData } from "sigma/types";
import type { EdgeEntry } from "graphology-types";
export type { Coordinates } from "sigma/types";

export interface HelvegNodeAttributes extends NodeDisplayData {
    style: string;
    icon: string;
    iconSize: number;
    outlines: Outlines;
    fire: FireStatus;
    fixed: boolean;
    collapsed: boolean;
}

export interface HelvegEdgeAttributes extends EdgeDisplayData {
    relation: string;
}

export type HelvegGraph = Graph<Partial<HelvegNodeAttributes>, Partial<HelvegEdgeAttributes>>;

export function findRoots(graph: Graph, relation?: string) {
    let roots = new Set<string>();
    graph.forEachNode((node, attr) => {
        let hasParent = false;
        graph.forEachInboundEdge(node, (edge, attr, src, dst, srcAttr, dstAttr, undir) => {
            if (relation && attr.relation === relation) {
                hasParent = true;
            }
        });

        if (!hasParent) {
            roots.add(node);
        }
    });

    return roots;
}

export function collapseNode(graph: Graph, nodeId: string, relation?: string) {
    // assume that if a node is collapsed, its children are hidden
    if (graph.getNodeAttribute(nodeId, "collapsed")) {
        return;
    }

    graph.setNodeAttribute(nodeId, "collapsed", true);
    graph.forEachOutboundEdge(nodeId, (edge, attr, src, dst, srcAttr, dstAttr, undir) => {
        if (relation && attr.relation !== relation) {
            return;
        }

        collapseNode(graph, dst);
        dstAttr.hidden = true;
    });
}

export function expandNode(
    graph: HelvegGraph,
    nodeId: string,
    recursive: boolean = false,
    relation?: string
) {
    let nodeSize = graph.getNodeAttribute(nodeId, "size") ?? 2;
    let x = graph.getNodeAttribute(nodeId, "x");
    let y = graph.getNodeAttribute(nodeId, "y");
    graph.setNodeAttribute(nodeId, "collapsed", false);
    graph.setNodeAttribute(nodeId, "hidden", false);

    let neighborEdge: EdgeEntry<Partial<HelvegNodeAttributes>, Partial<HelvegEdgeAttributes>>[] = [];
    for (let edge of graph.outboundEdgeEntries(nodeId)) {
        if (!relation || edge.attributes.relation === relation) {
            neighborEdge.push(edge);
        }
    }

    neighborEdge.forEach((neighbor, i) => {
        if (x === undefined || y === undefined || nodeSize === undefined) {
            return;
        }

        let dist = (nodeSize + (neighbor.attributes.size ?? 2));
        neighbor.targetAttributes.hidden = false;
        neighbor.targetAttributes.x = x + dist * Math.cos((i / neighborEdge.length) * 2 * Math.PI);
        neighbor.targetAttributes.y = y + dist * Math.sin((i / neighborEdge.length) * 2 * Math.PI);

        if (recursive) {
            expandNode(graph, neighbor.target, true, relation);
        }
    });
}

export function toggleNode(graph: HelvegGraph, nodeId: string, relation?: string) {
    if (graph.getNodeAttribute(nodeId, "collapsed")) {
        expandNode(graph, nodeId, false, relation);
    } else {
        collapseNode(graph, nodeId, relation);
    }
}
