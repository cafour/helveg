import { Multigraph, MultigraphNode } from "./data-model.ts";
import { MULTIGRAPH_NODE_KEY } from "./graph.ts";

export interface Inspection {
    expression: InspectionExpression;
}

export interface InspectionExpression {
    tokens: InspectionToken[];
}

export interface InspectionToken {
    kind: InspectionTokenKind;
    text: string;
    associatedPropertyName?: string;
}

export enum InspectionTokenKind {
    Trivia = "trivia",
    Keyword = "keyword",
    Type = "type",
    Identifier = "identifier",
}

export type Inspector = (graph: Multigraph, node: MultigraphNode) => Inspection;

export const FALLBACK_INSPECTOR: Inspector = (_graph, node) => {
    const name = node.name ?? (node as any)[MULTIGRAPH_NODE_KEY] ?? "Unknown";
    return {
        expression: {
            tokens: [
                {
                    kind: InspectionTokenKind.Identifier,
                    text: name,
                    associatedPropertyName: node.name ? "name" : undefined
                }
            ]
        }
    };
}
