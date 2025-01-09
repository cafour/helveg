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
    return {
        expression: {
            tokens: [
                name(node, InspectionTokenKind.Identifier, "name"),
                trivia(" ("),
                keyword(node.kind ?? "unknown", "kind"),
                trivia(")")
            ]
        }
    };
}

export const SPACE: Readonly<InspectionToken> = trivia(" ");

export type TokenFactory = (text: string, associatedPropertyName?: string) => InspectionToken;

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

export const MISSING_NAME = "<missing>";

export function name(
    node: MultigraphNode,
    kind: InspectionTokenKind = InspectionTokenKind.Identifier,
    associatedPropertyName: string = "name"
): InspectionToken {
    return {
        text: node.name ?? MISSING_NAME,
        kind: kind,
        associatedPropertyName: associatedPropertyName
    };
}
