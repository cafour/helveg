import { hierarchy, HierarchyNode } from "../deps/d3-hierarchy.ts";
import { Attributes, EdgeEntry, GraphEvents } from "../deps/graphology.ts";
import Graph from "../deps/graphology.ts";
import { NodeDisplayData, EdgeDisplayData, Sigma, NodeProgramType, NodeLabelDrawingFunction, NodeHoverDrawingFunction } from "../deps/sigma.ts";
import { AbstractNodeProgram, WorkaroundNodeProgram } from "../rendering/workaround_node.ts";
import { DataModel, Multigraph, MultigraphEdge, MultigraphNode, MultigraphNodeDiffStatus } from "./data-model.ts";
import { Outlines, FireStatus, Slices, Contour } from "./style.ts";

export interface HelvegNodeAttributes extends NodeDisplayData, Attributes {
    id: string;
    model: MultigraphNode;
    style?: string;
    kind?: string;
    icon?: string;
    baseSize?: number;
    iconSize?: number;
    outlines?: Outlines;
    slices?: Slices;
    fire?: FireStatus;
    fixed?: boolean;
    collapsed?: boolean;
    diff?: MultigraphNodeDiffStatus;
    inInitialPosition?: boolean;
    backgroundColor?: string;
    childCount?: number;
    descendantCount?: number;
    contour?: Contour;
    [key: string]: unknown;
}

export interface HelvegEdgeAttributes extends Partial<EdgeDisplayData>, Attributes {
    model?: MultigraphEdge;
    relation?: string;
    style?: string;
}

export interface HelvegGraphAttributes extends Attributes {
    model: DataModel;
    roots?: Set<string>;
}

export type HelvegGraph = Graph<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes>;

export const EMPTY_GRAPH: Readonly<HelvegGraph> = new Graph<
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
>();

// HACK: Sigma does not allow to disable hovering on nodes, so we have to track it ourselves.
export const isHoverEnabledSymbol = Symbol("isHoverEnabled");
export const hoveredNodeSymbol = Symbol("hoveredNode");

export type HelvegSigma = Sigma<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes> & {
    [isHoverEnabledSymbol]: boolean;
    [hoveredNodeSymbol]: string | null;
};

export type HelvegAbstractNodeProgram = AbstractNodeProgram<
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
>;

export type HelvegNodeProgramType = NodeProgramType<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes>;

export abstract class HelvegNodeProgram<Uniform extends string> extends WorkaroundNodeProgram<
    Uniform,
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
> {}

export type HelvegNodeLabelDrawingFunction = NodeLabelDrawingFunction<
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
>;

export type HelvegNodeHoverDrawingFunction = NodeHoverDrawingFunction<
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
>;

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

export interface ExpandNodeOptions {
    recursive?: boolean;
    relation?: string;
    shouldExpandSingleChildren?: boolean;
}

export const DEFAULT_EXPAND_NODE_OPTIONS: ExpandNodeOptions = {
    recursive: false,
    relation: undefined,
    shouldExpandSingleChildren: true,
};

export function expandNode(graph: HelvegGraph, nodeId: string, options?: ExpandNodeOptions) {
    options = { ...DEFAULT_EXPAND_NODE_OPTIONS, ...options };
    let nodeSize = graph.getNodeAttribute(nodeId, "size") ?? 2;
    let x = graph.getNodeAttribute(nodeId, "x");
    let y = graph.getNodeAttribute(nodeId, "y");
    graph.setNodeAttribute(nodeId, "collapsed", false);
    graph.setNodeAttribute(nodeId, "hidden", false);

    let neighborEdges: EdgeEntry<Partial<HelvegNodeAttributes>, Partial<HelvegEdgeAttributes>>[] = [];
    for (let edge of graph.outboundEdgeEntries(nodeId)) {
        if (!options.relation || edge.attributes.relation === options.relation) {
            neighborEdges.push(edge);
        }
    }

    neighborEdges.forEach((neighbor, i) => {
        if (x === undefined || y === undefined || nodeSize === undefined) {
            return;
        }

        let dist = nodeSize + (neighbor.attributes.size ?? 2);
        neighbor.targetAttributes.hidden = false;
        if (
            neighbor.sourceAttributes.inInitialPosition !== true &&
            neighbor.targetAttributes.inInitialPosition !== true
        ) {
            neighbor.targetAttributes.x = x + dist * Math.cos((i / neighborEdges.length) * 2 * Math.PI);
            neighbor.targetAttributes.y = y + dist * Math.sin((i / neighborEdges.length) * 2 * Math.PI);
            neighbor.targetAttributes.inInitialPosition = false;
        }

        const shouldExpandChild =
            options.recursive || (neighborEdges.length === 1 && options.shouldExpandSingleChildren);
        if (shouldExpandChild) {
            expandNode(graph, neighbor.target, options);
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
            expandNode(graph, s, { relation });
            expandPathsTo(graph, s, relation);
        }
    });
}

export function toggleNode(graph: HelvegGraph, nodeId: string, relation?: string, shouldExpand?: boolean) {
    shouldExpand ??= graph.getNodeAttribute(nodeId, "collapsed");
    if (shouldExpand) {
        expandNode(graph, nodeId, { relation });
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
        .filter((n) => n.kind)
        .map((n) => n.kind!)
        .filter((v, i, a) => a.indexOf(v) === i)
        .sort();
}

export interface HelvegTree {
    id: string;
    node: HelvegNodeAttributes;
    children?: HelvegTree[];
}

export interface HelvegForest {
    roots: HelvegTree[];
}

export function getForest(graph: HelvegGraph | undefined, relation: string, nodeKindOrder?: string[]): HelvegForest {
    if (graph === undefined) {
        return { roots: [] };
    }

    const rootIds = findRoots(graph, relation);
    const roots: HelvegTree[] = [];
    for (const rootId of rootIds) {
        const root = hierarchy(
            rootId,
            (nodeId) =>
                <string[]>(
                    graph
                        .mapOutboundEdges(nodeId, (_edge, attr, _src, dst, _srcAttr, _dstAttr) =>
                            attr.relation && attr.relation === relation ? dst : undefined
                        )
                        .filter((dst) => dst != undefined)
                )
        );

        function convertD3Node(node: HierarchyNode<string>): HelvegTree {
            let children = undefined;
            if (node.children) {
                children = node.children
                    .map(convertD3Node)
                    .sort((a, b) => (a.node.label ?? "").localeCompare(b.node.label ?? ""));
                if (nodeKindOrder && nodeKindOrder.length > 0) {
                    children.sort((a, b) => nodeKindOrder.indexOf(a.node.kind!) - nodeKindOrder.indexOf(b.node.kind!));
                }
            }
            return {
                id: node.data,
                node: graph!.getNodeAttributes(node.data),
                children: children,
            };
        }

        roots.push(convertD3Node(root));
    }

    return { roots };
}

export interface HelvegForestItem {
    id: string;
    node: HelvegNodeAttributes;
    parent?: HelvegForestItem;
    children?: HelvegForestItem[];
    depth: number;
}

export function getForestItems(forest: HelvegForest): HelvegForestItem[] {
    const items: HelvegForestItem[] = [];

    function visit(node: HelvegTree, depth: number = 0): HelvegForestItem {
        const item: HelvegForestItem = {
            id: node.id,
            node: node.node,
            depth: depth,
        };
        items.push(item);
        item.children = node.children === undefined ? undefined : node.children.map((c) => visit(c, depth + 1));
        item.children?.forEach((c) => (c.parent = item));
        return item;
    }

    forest.roots.forEach((t) => visit(t));
    return items;
}

type HelvegGraphEvent = keyof GraphEvents<HelvegGraph>;

const HELVEG_GRAPH_EVENTS: HelvegGraphEvent[] = [
    "nodeAdded",
    "edgeAdded",
    "nodeDropped",
    "edgeDropped",
    "cleared",
    "edgesCleared",
    "attributesUpdated",
    "nodeAttributesUpdated",
    "edgeAttributesUpdated",
    "eachNodeAttributesUpdated",
    "eachEdgeAttributesUpdated",
];

export function getAllGraphListeners(graph: HelvegGraph): Record<HelvegGraphEvent, any[]> {
    const listeners: Record<string, any[]> = {};
    for (const event of HELVEG_GRAPH_EVENTS) {
        listeners[event] = graph.rawListeners(event);
    }

    return listeners;
}

export function removeAllGraphListeners(graph: HelvegGraph): void {
    for (const event of HELVEG_GRAPH_EVENTS) {
        graph.removeAllListeners(event);
    }
}

export function setAllGraphListeners(graph: HelvegGraph, listeners: Record<HelvegGraphEvent, any[]>): void {
    for (const event of HELVEG_GRAPH_EVENTS) {
        for (const listener of listeners[event]) {
            graph.addListener(event, listener);
        }
    }
}

export function dropNode(graph: HelvegGraph, node: string) {
    graph.forEachInboundEdge((ie, iea, is, it) => {
        if (iea.relation == null) {
            return;
        }

        const relationModel = graph.getAttributes().model!.data!.relations[iea.relation];
        if (!relationModel.isTransitive) {
            return;
        }

        graph.forEachOutboundEdge(it, (oe, oea, _os, ot, _osa, _ota, undirected) => {
            if (ie === oe || oea.relation !== iea.relation) {
                return;
            }

            const edgeKey = `${iea.relation};${is};${ot}`;
            if (graph.hasEdge(edgeKey)) {
                return;
            }

            if (undirected) {
                graph.addUndirectedEdgeWithKey(edgeKey, is, ot, {
                    relation: iea.relation,
                });
            } else {
                graph.addDirectedEdgeWithKey(edgeKey, is, ot, {
                    relation: iea.relation,
                });
            }
        });
    });
}
