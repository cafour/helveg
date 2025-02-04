import forceAtlas2, { ForceAtlas2Settings, inferSettings } from "../deps/graphology-layout-forceatlas2.ts";
import Graph from "../deps/graphology.ts";
import { Sigma, DEFAULT_SETTINGS, NodeProgramType } from "../deps/sigma.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { DataModel, Multigraph, MultigraphRelation } from "../model/data-model.ts";
import {
    HelvegEdgeAttributes,
    HelvegGraph,
    HelvegGraphAttributes,
    HelvegNodeAttributes,
    collapseNode,
    dropNode,
    expandNode,
    findRoots,
} from "../model/graph.ts";
import { ILogger, Logger, sublogger } from "../model/logger.ts";
import {
    EdgeStylist,
    FALLBACK_EDGE_STYLE,
    FALLBACK_NODE_STYLE,
    NodeStyle,
    NodeStylist,
    OutlineStyle,
    Outlines,
    RelationStylist,
} from "../model/style.ts";
import { bfsGraph, bfsMultigraph } from "../model/traversal.ts";
import { GlyphProgramOptions, SizingMode } from "../rendering/node.glyph.ts";
import { WorkaroundNodeProgram } from "../rendering/workaround_node.ts";

export function initializeSupervisor(
    graph: HelvegGraph,
    onSupervisorProgress: (progress: ForceAtlas2Progress) => void,
    onSupervisorStopped: () => void,
    settings?: ForceAtlas2Settings,
    logger?: ILogger
): ForceAtlas2Supervisor {
    settings ??= { ...inferSettings(graph), adjustSizes: true };
    const supervisor = new ForceAtlas2Supervisor(graph, settings, logger ? sublogger(logger, "fa2") : undefined);
    supervisor.progress.subscribe(onSupervisorProgress);
    supervisor.stopped.subscribe(onSupervisorStopped);
    return supervisor;
}

// HACK: Sigma does not allow to disable hovering on nodes, so we have to track it ourselves.
export const isHoverEnabledSymbol = Symbol("isHoverEnabled");
export const hoveredNodeSymbol = Symbol("hoveredNode");

export type HelvegSigma = Sigma<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes> & {
    [isHoverEnabledSymbol]: boolean;
    [hoveredNodeSymbol]: string | null;
};

export type HelvegNodeProgramType = NodeProgramType<HelvegNodeAttributes, HelvegEdgeAttributes>;

export abstract class HelvegNodeProgram<Uniform extends string> extends WorkaroundNodeProgram<
    Uniform,
    HelvegNodeAttributes,
    HelvegEdgeAttributes,
    HelvegGraphAttributes
> {}

export function initializeSigma(
    element: HTMLElement,
    graph: HelvegGraph,
    glyphProgram: HelvegNodeProgramType
): HelvegSigma {
    const sigma = new Sigma(graph, element, {
        nodeProgramClasses: {
            glyph: glyphProgram,
        },
        labelFont: "'Cascadia Mono', 'Consolas', monospace",
        edgeLabelFont: "'Cascadia Mono', 'Consolas', monospace",
        itemSizesReference: "positions",
        zoomToSizeRatioFunction: (ratio) => ratio,
    }) as HelvegSigma;
    sigma[isHoverEnabledSymbol] = false;
    sigma[hoveredNodeSymbol] = null;

    sigma.on("enterNode", (e) => {
        if (sigma[isHoverEnabledSymbol]) {
            sigma[hoveredNodeSymbol] = e.node;
        } else {
            sigma[hoveredNodeSymbol] = null;
            // HACK: This is IMHO currently the only way to force Sigma *not to* render hovered nodes.
            (sigma as any).hoveredNode = null;
        }
    });

    sigma.on("leaveNode", (e) => {
        if (sigma[isHoverEnabledSymbol]) {
            sigma[hoveredNodeSymbol] = null;
            // HACK: This is IMHO currently the only way to force Sigma *not to* render hovered nodes.
            (sigma as any).hoveredNode = null;
        }
    });

    sigma.getMouseCaptor().on("doubleClick", (e) => {
        e.preventSigmaDefault();
    });
    sigma.getTouchCaptor().on("doubletap", (e) => {
        e.preventSigmaDefault();
    });

    return sigma;
}

export function configureSigma(sigma: HelvegSigma, options: GlyphProgramOptions) {
    sigma.setSetting("renderLabels", options.showLabels);
    if (options.isPizzaEnabled) {
        // sigma.setSetting("zoomToSizeRatioFunction", (cameraRatio) => cameraRatio);
        sigma.setSetting("nodeHoverProgramClasses", {});
        sigma[isHoverEnabledSymbol] = false;
    } else {
        // sigma.setSetting("zoomToSizeRatioFunction", DEFAULT_SETTINGS.zoomToSizeRatioFunction);
        sigma.setSetting("nodeHoverProgramClasses", DEFAULT_SETTINGS.nodeHoverProgramClasses);
        sigma[isHoverEnabledSymbol] = true;
    }
}

export function initializeGraph(
    modelGraph: HelvegGraph,
    mainRelation?: string,
    selectedRelations?: string[],
    selectedKinds?: string[],
    expandedDepth?: number
): HelvegGraph {
    const graph = modelGraph.copy();

    if (selectedKinds != null) {
        modelGraph.forEachNode((n, na) => {
            if (na.kind == null || !selectedKinds.includes(na.kind)) {
                dropNode(modelGraph, n);
            }
        });
    }

    if (selectedRelations != null) {
        modelGraph.forEachEdge((e, ea) => {
            if (ea.relation == null || !selectedRelations.includes(ea.relation)) {
                graph.dropEdge(e);
            }
        });
    }

    if (mainRelation !== undefined && expandedDepth !== undefined && expandedDepth >= 0) {
        const mainRoots = findRoots(graph, mainRelation);
        mainRoots.forEach((r) => {
            collapseNode(graph, r, mainRelation);
            bfsGraph(graph, r, {
                maxDepth: expandedDepth - 1,
                callback: (n, _a, d) => {
                    expandNode(graph, n, { relation: mainRelation });
                },
            });
        });
    }

    return graph;
}

function addRegularRelation(graph: HelvegGraph, multigraph: Multigraph, relationId: string) {
    const relation = multigraph.relations[relationId];
    if (!relation || !relation.edges) {
        return;
    }

    for (let [id, edge] of Object.entries(relation.edges)) {
        if (graph.hasNode(edge.src) && graph.hasNode(edge.dst)) {
            graph.addDirectedEdgeWithKey(`${relationId};${id}`, edge.src, edge.dst, {
                relation: relationId,
            });
        }
    }
}

function addTransitiveRelation(graph: HelvegGraph, multigraph: Multigraph, relationId: string) {
    const relation = multigraph.relations[relationId];
    if (!relation || !relation.edges) {
        return;
    }

    addRegularRelation(graph, multigraph, relationId);

    graph.forEachNode((id, a) => {
        // find nodes that are reachable from the current node, stop at those that are already in the graph
        const transitiveChildren = bfsMultigraph(multigraph, id, {
            relation: relationId,
            callback: (n) => n === id || !graph.hasNode(n),
        });

        // add the transitive edges and remove the unincluded nodes
        transitiveChildren.forEach((child) => {
            if (child === id) {
                // the node doesn't have an edge to itself
                return;
            }

            if (graph.hasNode(child)) {
                let edgeKey = `declares;${id};${child}`;
                if (!graph.hasEdge(edgeKey)) {
                    graph.addDirectedEdgeWithKey(edgeKey, id, child, {
                        relation: relationId,
                    });
                }
            }
        });
    });
}

export function toHelvegNodeAttributes(
    glyphProgramOptions: GlyphProgramOptions,
    nodeStyle: NodeStyle
): Partial<HelvegNodeAttributes> {
    const attributes: Partial<HelvegNodeAttributes> = {};

    const outlines = [
        { width: nodeStyle.size, style: OutlineStyle.Solid },
        ...nodeStyle.outlines.slice(0, 3),
    ] as Outlines;

    attributes.baseSize = nodeStyle.size;
    attributes.size = attributes.baseSize;
    // attributes.size = glyphProgramOptions.showOutlines && outlines.length > 0
    //     ? getOutlinesTotalWidth(outlines)
    //     : nodeStyle.size;
    if (nodeStyle.slices?.width > 0) {
        attributes.size += nodeStyle.slices.width + glyphProgramOptions.gap;
    }
    attributes.iconSize = nodeStyle.size;

    const getSize = (sizingMode: SizingMode, value: number) => {
        switch (sizingMode) {
            case "linear":
                // keep it as is
                return value;
            case "sqrt":
                return Math.sqrt(value);
            case "log":
                return Math.log(Math.max(value, 1));
            default:
                return value;
        }
    };

    attributes.size = getSize(glyphProgramOptions.sizingMode, attributes.size);
    attributes.iconSize = getSize(glyphProgramOptions.sizingMode, attributes.iconSize);
    attributes.baseSize = getSize(glyphProgramOptions.sizingMode, attributes.baseSize);
    attributes.color = nodeStyle.color;
    attributes.backgroundColor = nodeStyle.backgroundColor;
    attributes.type = "glyph";
    attributes.icon = nodeStyle.icon;
    attributes.outlines = outlines;
    attributes.slices = nodeStyle.slices;
    attributes.fire = nodeStyle.fire;
    attributes.contour = nodeStyle.contour;

    return attributes;
}

export function styleGraph(
    graph: HelvegGraph,
    model: DataModel,
    glyphProgramOptions: GlyphProgramOptions,
    nodeStylist?: NodeStylist,
    relationStylist?: RelationStylist,
    edgeStylist?: EdgeStylist,
    logger?: ILogger
) {
    graph.forEachNode((node, attributes) => {
        if (!model.data || !model.data.nodes[node]) {
            logger?.debug(`Node '${node}' does not exist in the model.`);
            return;
        }

        let nodeStyle = { ...FALLBACK_NODE_STYLE };
        if (nodeStylist) {
            nodeStyle = { ...nodeStyle, ...nodeStylist(model.data.nodes[node]) };
        }

        Object.assign(attributes, toHelvegNodeAttributes(glyphProgramOptions, nodeStyle));
    });

    graph.forEachEdge((edge, attributes) => {
        if (!attributes.relation || !model.data) {
            return;
        }

        const relation = model.data.relations[attributes.relation];
        if (!relation || !relation.edges) {
            return;
        }

        let edgeStyle = { ...FALLBACK_EDGE_STYLE };

        if (relationStylist) {
            edgeStyle = { ...edgeStyle, ...relationStylist(attributes.relation) };
        }

        if (edgeStylist) {
            edgeStyle = { ...edgeStyle, ...edgeStylist(attributes.relation, relation.edges[edge]) };
        }

        attributes.type = edgeStyle.type;
        attributes.color = edgeStyle.color;
        attributes.size = edgeStyle.width;
    });
}
