import { hierarchy } from "../deps/d3-hierarchy.ts";
import Graph from "../deps/graphology.ts";
import { DataModel, Multigraph } from "./data-model.ts";
import { findRoots, HelvegEdgeAttributes, HelvegGraph, HelvegGraphAttributes, HelvegNodeAttributes } from "./graph.ts";

export interface PreprocessOptions {
    mainRelation: string;
}

export const DEFAULT_PREPROCESS_OPTIONS: PreprocessOptions = {
    mainRelation: "declares",
};

export type PreprocessFunc = (model: DataModel, options?: Partial<PreprocessOptions>) => HelvegGraph;

export function preprocess(model: DataModel, options?: Partial<PreprocessOptions>): HelvegGraph {
    options = { ...DEFAULT_PREPROCESS_OPTIONS, ...options };
    const graph: HelvegGraph = new Graph<HelvegNodeAttributes, HelvegEdgeAttributes, HelvegGraphAttributes>({
        multi: true,
        allowSelfLoops: true,
        type: "directed",
    });

    if (!model.data) {
        return graph;
    }

    for (const nodeId of Object.keys(model.data.nodes)) {
        const node = model.data.nodes[nodeId];
        node.errorCount = node.diagnostics?.filter((d) => d.severity === "error").length ?? 0;
        node.hasErrors = node.errorCount > 0;
        node.warningCount = node.diagnostics?.filter((d) => d.severity === "warning").length ?? 0;
        node.hasWarnings = node.warningCount > 0;
        node.commentCount = node.comments?.length ?? 0;
        node.hasComments = node.commentCount > 0;

        graph.addNode(nodeId, {
            id: nodeId,
            model: node,
            label: node.name ?? nodeId,
            x: 0,
            y: 0,
            kind: node.kind,
            diff: node.diff,
        });
    }

    for (const relationId of Object.keys(model.data.relations)) {
        addRelation(graph, model.data, relationId);
    }

    if (options.mainRelation != null) {
        const roots = findRoots(graph, options.mainRelation);
        graph.getAttributes().roots = roots;
        for (const root of roots) {
            const rootHierarchy = hierarchy(
                root,
                (nodeId) =>
                    <string[]>(
                        graph
                            .mapOutboundEdges(nodeId, (_edge, attr, _src, dst, _srcAttr, dstAttr) =>
                                attr.relation === options.mainRelation ? dst : undefined
                            )
                            .filter((dst) => dst != undefined)
                    )
            );
            rootHierarchy.each(node => {
                const attr = graph.getNodeAttributes(node.data);
                attr.model!["childCount"] = node.children?.length ?? 0;
                attr.model!["descendantCount"] = node.descendants().length;
            })
        }
    }

    return graph;
}

function addRelation(graph: HelvegGraph, multigraph: Multigraph, relationId: string) {
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
