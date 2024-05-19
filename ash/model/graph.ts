import { Attributes, EdgeEntry } from "../deps/graphology.ts";
import Graph from "../deps/graphology.ts";
import { Multigraph, MultigraphNodeDiffStatus } from "./data-model.ts";
import { Outlines, FireStatus } from "./style.ts";

export interface HelvegNodeAttributes extends Attributes {
    style?: string;
    kind?: string;
    icon?: string;
    iconSize?: number;
    outlines?: Outlines;
    fire?: FireStatus;
    fixed?: boolean;
    collapsed?: boolean;
    diff?: MultigraphNodeDiffStatus;
    inInitialPosition?: boolean;
}

export interface HelvegEdgeAttributes extends Attributes {
    relation?: string;
    style?: string;
}

export type HelvegGraph = Graph<HelvegNodeAttributes, HelvegEdgeAttributes>;

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

        collapseNode(graph, dst, relation);
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
        if (neighbor.sourceAttributes.inInitialPosition !== true
            && neighbor.targetAttributes.inInitialPosition !== true) {
            neighbor.targetAttributes.x = x + dist * Math.cos((i / neighborEdge.length) * 2 * Math.PI);
            neighbor.targetAttributes.y = y + dist * Math.sin((i / neighborEdge.length) * 2 * Math.PI);
            neighbor.targetAttributes.inInitialPosition = false;
        }

        if (recursive) {
            expandNode(graph, neighbor.target, true, relation);
        }
    });
}

export function expandPathsTo(graph: HelvegGraph, nodeId: string, relation?: string) {
    graph.forEachInboundEdge(nodeId, (e, a, s, t, sa, ta) => {
        if (s === nodeId) {
            // ignore self-references
            return;
        }
        if (sa.collapsed === true && (!relation || a.relation === relation)) {
            expandNode(graph, s, false, relation);
            expandPathsTo(graph, s, relation);
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

export function getRelations(graph: Multigraph | null | undefined): string[] {
    if (!graph || !graph.relations) {
        return [];
    }

    return Object.keys(graph.relations).sort();
}

export function getNodeKinds(graph: Multigraph | null | undefined): string[] {
    if (!graph || !graph.nodes) {
        return [];
    }

    return Object.values(graph.nodes)
        .filter(n => n.kind)
        .map(n => n.kind!)
        .filter((v, i, a) => a.indexOf(v) === i)
        .sort();
}
