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

export const SPACE: Readonly<InspectionToken> = trivia(" ");

export function trivia(text: string, associatedPropertyName?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Trivia,
        text: text,
        associatedPropertyName: associatedPropertyName
    };
}

export function keyword(text: string, associatedPropertyName?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Keyword,
        text: text,
        associatedPropertyName: associatedPropertyName
    };
}

export function identifier(text: string, associatedPropertyName?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Identifier,
        text: text,
        associatedPropertyName: associatedPropertyName
    };
}

export function type(text: string, associatedPropertyName?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Type,
        text: text,
        associatedPropertyName: associatedPropertyName
    };
}
