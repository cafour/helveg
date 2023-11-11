import forceAtlas2 from "../deps/graphology-layout-forceatlas2.ts";
import Graph from "../deps/graphology.ts";
import { Sigma, Coordinates, DEFAULT_SETTINGS, NodeProgramConstructor, SigmaNodeEventPayload, SigmaStageEventPayload } from "../deps/sigma.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { DataModel, Multigraph, MultigraphRelation } from "../model/data-model.ts";
import { HelvegEdgeAttributes, HelvegGraph, HelvegNodeAttributes } from "../model/graph.ts";
import { ILogger, Logger, sublogger } from "../model/logger.ts";
import { EdgeStylist, NodeStylist, OutlineStyle, Outlines, getOutlinesTotalWidth } from "../model/style.ts";
import { bfsMultigraph } from "../model/traversal.ts";
import { GlyphProgramOptions } from "../rendering/node.glyph.ts";

export function initializeSupervisor(
    graph: HelvegGraph,
    onSupervisorProgress: (progress: ForceAtlas2Progress) => void,
    logger?: ILogger
): ForceAtlas2Supervisor {

    let settings = forceAtlas2.inferSettings(graph);
    settings.adjustSizes = true;
    const supervisor = new ForceAtlas2Supervisor(graph, settings, logger ? sublogger(logger, "fa2") : undefined);
    supervisor.progress.subscribe(onSupervisorProgress);
    return supervisor;
}

// HACK: Sigma does not allow to disable hovering on nodes, so we have to track it ourselves.
export const isHoverEnabledSymbol = Symbol("isHoverEnabled");
export const hoveredNodeSymbol = Symbol("hoveredNode");

export type HelvegSigma = Sigma<HelvegGraph> & { [isHoverEnabledSymbol]: boolean, [hoveredNodeSymbol]: string | null };

export function initializeSigma(
    element: HTMLElement,
    graph: HelvegGraph,
    glyphProgram: NodeProgramConstructor,
    onClick?: (payload: SigmaNodeEventPayload) => void,
    onNodeDown?: (payload: SigmaNodeEventPayload) => void,
    onStageDown?: (payload: SigmaStageEventPayload) => void,
    onDown?: (coords: Coordinates) => void,
    onUp?: (coords: Coordinates) => void,
    onMove?: (coords: Coordinates) => boolean | void
): HelvegSigma {

    const sigma = new Sigma(graph, element, {
        nodeProgramClasses: {
            glyph: glyphProgram,
        },
        labelFont: "'Cascadia Mono', 'Consolas', monospace",
        edgeLabelFont: "'Cascadia Mono', 'Consolas', monospace",
        itemSizesReference: "positions"
    }) as HelvegSigma;
    sigma[isHoverEnabledSymbol] = false;
    sigma[hoveredNodeSymbol] = null;

    if (onClick) {
        sigma.on("clickNode", onClick);
    }

    if (onNodeDown) {
        sigma.on("downNode", onNodeDown);
    }

    if (onStageDown) {
        sigma.on("downStage", onStageDown);
    }

    if (onDown) {
        sigma.getMouseCaptor().on("mousedown", e => onDown(e));
        sigma.getTouchCaptor().on("touchdown", e => onDown(e.touches[0]));
    }

    if (onUp) {
        sigma.getMouseCaptor().on("mouseup", onUp);
        sigma.getTouchCaptor().on("touchup", e => onUp(e.touches[0]));
    }

    if (onMove) {
        sigma.getMouseCaptor().on("mousemovebody", e => {
            if (onMove(e) === false) {
                // prevent Sigma from moving the camera
                e.preventSigmaDefault();
                e.original.preventDefault();
                e.original.stopPropagation();
            }

        });
        sigma.getTouchCaptor().on("touchmove", e => {
            if (e.touches.length == 1) {
                onMove(e.touches[0]);
                e.original.preventDefault();
            }
        });
    }

    sigma.on("enterNode", e => {
        if (sigma[isHoverEnabledSymbol]) {
            sigma[hoveredNodeSymbol] = e.node;
        } else {
            sigma[hoveredNodeSymbol] = null;
        }
    })

    return sigma;
}

export function configureSigma(
    sigma: HelvegSigma,
    options: GlyphProgramOptions
) {
    sigma.setSetting("renderLabels", options.showLabels);
    if (options.isPizzaEnabled) {
        sigma.setSetting("zoomToSizeRatioFunction", (cameraRatio) => cameraRatio);
        sigma.setSetting("hoverRenderer", () => { });
        sigma[isHoverEnabledSymbol] = false;
    } else {
        sigma.setSetting("zoomToSizeRatioFunction", DEFAULT_SETTINGS.zoomToSizeRatioFunction);
        sigma.setSetting("hoverRenderer", DEFAULT_SETTINGS.hoverRenderer);
        sigma[isHoverEnabledSymbol] = true;
    }
}

export function initializeGraph(
    model: DataModel,
    selectedRelations?: string[],
    selectedKinds?: string[],
    logger?: ILogger
): HelvegGraph {

    const graph = new Graph<HelvegNodeAttributes, HelvegEdgeAttributes>({
        multi: true,
        allowSelfLoops: true,
        type: "directed"
    });

    if (!model.data) {
        return graph;
    }

    for (const nodeId in model.data.nodes) {
        const node = model.data.nodes[nodeId];
        if (!node.kind || !selectedKinds || selectedKinds.includes(node.kind)) {
            graph.addNode(nodeId, {
                label: node.name ?? nodeId,
                x: 0,
                y: 0
            });
        }
    }

    selectedRelations ??= Object.keys(model.data.relations);
    for (const relationId of selectedRelations) {
        const relation = model.data.relations[relationId];
        if (!relation) {
            continue;
        }

        if (relation.isTransitive) {
        // if (false) {
            addTransitiveRelation(graph, model.data, relationId);
        } else {
            addRegularRelation(graph, model.data, relationId);
        }
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
                relation: relationId
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
            callback: n => n === id || !graph.hasNode(n)
        });
        
        // add the transitive edges and remove the unincluded nodes
        transitiveChildren.forEach(child => {
            if (child === id) {
                // the node doesn't have an edge to itself
                return;
            }

            if (graph.hasNode(child)) {
                let edgeKey = `declares;${id};${child}`;
                if (!graph.hasEdge(edgeKey)) {
                    graph.addDirectedEdgeWithKey(edgeKey, id, child, {
                        relation: relationId
                    });
                }
            }
        });
    });
}

export function styleGraph(
    graph: HelvegGraph,
    model: DataModel,
    glyphProgramOptions: GlyphProgramOptions,
    nodeStylist: NodeStylist,
    edgeStylist: EdgeStylist,
    logger?: ILogger) {

    graph.forEachNode((node, attributes) => {

        if (!model.data || !model.data.nodes[node]) {
            logger?.debug(`Node '${node}' does not exist in the model.`);
            return;
        }

        const nodeStyle = nodeStylist(model.data.nodes[node]);
        if (!nodeStyle) {
            logger?.debug(`Node style '${attributes.style}' could not be applied to node '${node}'.`);
        }

        const outlines = [
            { width: nodeStyle.size, style: OutlineStyle.Solid },
            ...nodeStyle.outlines.slice(0, 3),
        ] as Outlines;
        attributes.size = glyphProgramOptions.showOutlines && outlines.length > 0
            ? getOutlinesTotalWidth(outlines)
            : nodeStyle.size;
        attributes.iconSize = nodeStyle.size;
        attributes.color = nodeStyle.color;
        attributes.type = "glyph";
        attributes.icon = nodeStyle.icon;
        attributes.outlines = outlines;
        attributes.fire = nodeStyle.fire;
    });

    graph.forEachEdge((edge, attributes) => {
        if (!attributes.style || !attributes.relation || !model.data) {
            return;
        }

        const relation = model.data.relations[attributes.relation];
        if (!relation || !relation.edges) {
            return;
        }

        const edgeStyle = edgeStylist(
            attributes.relation,
            relation.edges[edge]
        );
        if (!edgeStyle) {
            logger?.debug(`Edge style '${attributes.style}' could not be applied to edge '${edge}'.`);
            return;
        }

        attributes.type = edgeStyle.type;
        attributes.color = edgeStyle.color;
        attributes.size = edgeStyle.width;
    });
}
