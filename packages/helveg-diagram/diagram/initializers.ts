import { ForceAtlas2Settings, inferSettings } from "../deps/graphology-layout-forceatlas2.ts";
import { Sigma, DEFAULT_SETTINGS, NodeProgramType, AbstractNodeProgram } from "../deps/sigma.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { DataModel, Multigraph } from "../model/data-model.ts";
import {
    HelvegEdgeAttributes,
    HelvegGraph,
    HelvegGraphAttributes,
    HelvegNodeAttributes,
    HelvegNodeProgramType,
    HelvegSigma,
    collapseNode,
    dropNode,
    expandNode,
    findRoots,
    hoveredNodeSymbol,
    isHoverEnabledSymbol,
} from "../model/graph.ts";
import { ILogger, Logger, sublogger } from "../model/logger.ts";
import {
    DiagnosticIndicatorStyle,
    EdgeStylist,
    FALLBACK_EDGE_STYLE,
    FALLBACK_NODE_STYLE,
    FireStatus,
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
    onSupervisorUpdated: () => void,
    settings?: ForceAtlas2Settings,
    logger?: ILogger
): ForceAtlas2Supervisor {
    settings ??= { ...inferSettings(graph), adjustSizes: true };
    const supervisor = new ForceAtlas2Supervisor(graph, settings, logger ? sublogger(logger, "fa2") : undefined);
    supervisor.progress.subscribe(onSupervisorProgress);
    supervisor.stopped.subscribe(onSupervisorStopped);
    supervisor.updated.subscribe(onSupervisorUpdated);
    return supervisor;
}

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

    // graph.removeAllListeners("eachNodeAttributesUpdated");

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
    modelGraph: Readonly<HelvegGraph>,
    mainRelation?: string,
    selectedRelations?: string[],
    selectedKinds?: string[],
    expandedDepth?: number
): HelvegGraph {
    const graph = modelGraph.copy();

    if (selectedKinds != null) {
        graph.forEachNode((n, na) => {
            if (na.kind == null || !selectedKinds.includes(na.kind)) {
                dropNode(graph, n);
            }
        });
    }

    if (selectedRelations != null) {
        graph.forEachEdge((e, ea) => {
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
    attributes.diagnosticIndicator = nodeStyle.diagnosticIndicator;

    return attributes;
}

export interface StyleGraphOptions {
    nodeStylist?: NodeStylist<any>;
    nodeStylistParams?: any;
    relationStylist?: RelationStylist<any>;
    relationStylistParams?: any;
    edgeStylist?: EdgeStylist<any>;
    edgeStylistParams?: any;
}

export function styleGraph(graph: HelvegGraph, glyphProgramOptions: GlyphProgramOptions, options?: StyleGraphOptions) {
    const model = graph.getAttribute("model");
    options ??= {};
    if (options.nodeStylist) {
        graph.forEachNode((_n, attributes) => {
            let nodeStyle = { ...FALLBACK_NODE_STYLE };
            if (options.nodeStylist) {
                nodeStyle = { ...nodeStyle, ...options.nodeStylist(attributes, options.nodeStylistParams) };
            }

            Object.assign(attributes, toHelvegNodeAttributes(glyphProgramOptions, nodeStyle));
        });
    }

    graph.forEachNode((_n, attributes) => {
        if (attributes.fire && attributes.fire !== FireStatus.None) {
            const indicator =
                attributes.fire === FireStatus.Flame
                    ? DiagnosticIndicatorStyle.ERROR
                    : DiagnosticIndicatorStyle.WARNING;
            let current: HelvegNodeAttributes | null = attributes;
            while (current != null) {
                if (
                    current.diagnosticIndicator === DiagnosticIndicatorStyle.ERROR ||
                    (indicator === DiagnosticIndicatorStyle.ERROR &&
                        current.diagnosticIndicator === DiagnosticIndicatorStyle.WARNING)
                ) {
                    break;
                }

                current.diagnosticIndicator = indicator;
                const parentEdge: string | undefined = graph.findInEdge(
                    current.id,
                    (_e, ea, s) => ea.relation === "declares"
                );
                current = parentEdge != null ? graph.getSourceAttributes(parentEdge) : null;
            }
        }
    });

    if (options.edgeStylist || options.relationStylist) {
        graph.forEachEdge((edge, attributes) => {
            if (!attributes.relation || !model.data) {
                return;
            }
            const relation = model.data.relations[attributes.relation];
            if (!relation || !relation.edges) {
                return;
            }

            let edgeStyle = { ...FALLBACK_EDGE_STYLE };

            if (options.relationStylist) {
                edgeStyle = {
                    ...edgeStyle,
                    ...options.relationStylist(attributes.relation, options.relationStylistParams),
                };
            }

            if (options.edgeStylist) {
                edgeStyle = {
                    ...edgeStyle,
                    ...options.edgeStylist(attributes.relation, relation.edges[edge], options.edgeStylistParams),
                };
            }

            attributes.type = edgeStyle.type;
            attributes.color = edgeStyle.color;
            attributes.size = edgeStyle.width;
        });
    }
}
