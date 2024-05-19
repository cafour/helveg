import forceAtlas2, { ForceAtlas2Settings, inferSettings } from "../deps/graphology-layout-forceatlas2.ts";
import Graph from "../deps/graphology.ts";
import { ForceAtlas2Progress, ForceAtlas2Supervisor } from "../layout/forceAltas2Supervisor.ts";
import { DataModel, Multigraph, MultigraphRelation } from "../model/data-model.ts";
import { HelvegEdgeAttributes, HelvegGraph, HelvegNodeAttributes, collapseNode, expandNode, findRoots, toggleNode } from "../model/graph.ts";
import { ILogger, Logger, sublogger } from "../model/logger.ts";
import { EdgeStylist, FALLBACK_EDGE_STYLE, FALLBACK_NODE_STYLE, NodeStylist, OutlineStyle, Outlines, RelationStylist, getOutlinesTotalWidth } from "../model/style.ts";
import { bfsGraph, bfsMultigraph } from "../model/traversal.ts";

export function initializeSupervisor(
    graph: HelvegGraph,
    onSupervisorProgress: (progress: ForceAtlas2Progress) => void,
    settings?: ForceAtlas2Settings,
    logger?: ILogger
): ForceAtlas2Supervisor {

    settings ??= { ...inferSettings(graph), adjustSizes: true };
    const supervisor = new ForceAtlas2Supervisor(graph, settings, logger ? sublogger(logger, "fa2") : undefined);
    supervisor.progress.subscribe(onSupervisorProgress);
    return supervisor;
}

export function initializeGraph(
    model: DataModel,
    mainRelation?: string,
    selectedRelations?: string[],
    selectedKinds?: string[],
    expandedDepth?: number
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
                y: 0,
                kind: node.kind,
                diff: node.diff
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
            addTransitiveRelation(graph, model.data, relationId);
        } else {
            addRegularRelation(graph, model.data, relationId);
        }
    }

    if (mainRelation !== undefined && expandedDepth !== undefined && expandedDepth >= 0) {
        const mainRoots = findRoots(graph, mainRelation);
        mainRoots.forEach(r => {
            collapseNode(graph, r, mainRelation);
            bfsGraph(graph, r, {
                maxDepth: expandedDepth - 1,
                callback: (n, _a, d) => {
                    expandNode(graph, n, false, mainRelation);
                }

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
    // glyphProgramOptions: GlyphProgramOptions,
    nodeStylist?: NodeStylist,
    relationStylist?: RelationStylist,
    edgeStylist?: EdgeStylist,
    logger?: ILogger) {

    graph.forEachNode((node, attributes) => {

        if (!model.data || !model.data.nodes[node]) {
            logger?.debug(`Node '${node}' does not exist in the model.`);
            return;
        }

        let nodeStyle = { ...FALLBACK_NODE_STYLE };
        if (nodeStylist) {
            nodeStyle = { ...nodeStyle, ...nodeStylist(model.data.nodes[node]) };
        }

        const outlines = [
            { width: nodeStyle.size, style: OutlineStyle.Solid },
            ...nodeStyle.outlines.slice(0, 3),
        ] as Outlines;

        // attributes.size = glyphProgramOptions.showOutlines && outlines.length > 0
        //     ? getOutlinesTotalWidth(outlines)
        //     : nodeStyle.size;
        attributes.iconSize = nodeStyle.size;

        // const getSize = (sizingMode: SizingMode, value: number) => {
        //     switch (sizingMode) {
        //         case "linear":
        //             // keep it as is
        //             return value;
        //         case "sqrt":
        //             return Math.sqrt(value);
        //         case "log":
        //             return Math.log(Math.max(value, 1));
        //         default:
        //             return value;
        //     }
        // }

        // attributes.size = getSize(glyphProgramOptions.sizingMode, attributes.size);
        // attributes.iconSize = getSize(glyphProgramOptions.sizingMode, attributes.iconSize);
        attributes.color = nodeStyle.color;
        attributes.type = "glyph";
        attributes.icon = nodeStyle.icon;
        attributes.outlines = outlines;
        attributes.fire = nodeStyle.fire;
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
