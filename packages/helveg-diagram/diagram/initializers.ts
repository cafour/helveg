import forceAtlas2 from "../deps/graphology-layout-forceatlas2.ts";
import Graph from "../deps/graphology.ts";
import Sigma, { Coordinates, DEFAULT_SETTINGS, NodeProgramConstructor, SigmaNodeEventPayload, SigmaStageEventPayload } from "../deps/sigma.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { HelvegEdgeAttributes, HelvegGraph, HelvegNodeAttributes } from "../model/graph.ts";
import { AppearanceOptions, DataOptions } from "../model/options.ts";
import { VisualizationModel } from "../model/visualization.ts";

export function initializeSupervisor(
    graph: HelvegGraph,
    onSupervisorProgress: (progress: ForceAtlas2Progress) => void
): ForceAtlas2Supervisor {

    let settings = forceAtlas2.inferSettings(graph);
    settings.adjustSizes = true;
    const supervisor = new ForceAtlas2Supervisor(graph, settings);
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
    appearanceOptions: AppearanceOptions
) {
    sigma.setSetting("renderLabels", appearanceOptions.glyph.showLabels);
    if (appearanceOptions.codePizza.isEnabled) {
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
    model: VisualizationModel,
    dataOptions: DataOptions,
): HelvegGraph {

    const graph = new Graph<HelvegNodeAttributes, HelvegEdgeAttributes>({
        multi: true,
        allowSelfLoops: true,
        type: "directed"
    });
    for (const nodeId in model.multigraph.nodes) {
        const node = model.multigraph.nodes[nodeId];
        graph.addNode(nodeId, {
            label: node.properties.Label ?? nodeId,
            x: 0,
            y: 0,
            style: node.properties.Style
        });
    }

    for (const relationId of dataOptions.selectedRelations) {
        const relation = model.multigraph.relations[relationId];
        if (!relation) {
            continue;
        }

        for (let [id, edge] of Object.entries(relation.edges)) {
            try {
                graph.addDirectedEdgeWithKey(`${relationId};${id}`, edge.src, edge.dst, {
                    relation: relationId,
                    style: edge.properties.Style
                });
            } catch (error) {
                DEBUG && console.warn(`Failed to add an edge. edge=${edge}, error=${error}`);
            }
        }
    }

    return graph;
}
