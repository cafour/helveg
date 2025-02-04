import { Multigraph, MultigraphNode } from "./data-model.ts";
import { HelvegGraph, HelvegGraphAttributes } from "./graph.ts";

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
    hint?: string;
}

export enum InspectionTokenKind {
    Trivia = "trivia",
    Keyword = "keyword",
    Type = "type",
    Identifier = "identifier",
}

export type Inspector = (graph: HelvegGraph, node: HelvegGraphAttributes) => Inspection;

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

export type TokenFactory = (text: string, associatedPropertyName?: string, hint?: string) => InspectionToken;

export function trivia(text: string, associatedPropertyName?: string, hint?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Trivia,
        text: text,
        associatedPropertyName: associatedPropertyName,
        hint: hint
    };
}

export function keyword(text: string, associatedPropertyName?: string, hint?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Keyword,
        text: text,
        associatedPropertyName: associatedPropertyName,
        hint: hint
    };
}

export function identifier(text: string, associatedPropertyName?: string, hint?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Identifier,
        text: text,
        associatedPropertyName: associatedPropertyName,
        hint: hint,
    };
}

export function type(text: string, associatedPropertyName?: string, hint?: string): InspectionToken {
    return {
        kind: InspectionTokenKind.Type,
        text: text,
        associatedPropertyName: associatedPropertyName,
        hint: hint
    };
}

export const MISSING_TEXT = "<missing>";

export function name(
    node: MultigraphNode,
    kind: InspectionTokenKind = InspectionTokenKind.Identifier,
    associatedPropertyName: string = "name"
): InspectionToken {
    return {
        text: node.name ?? MISSING_TEXT,
        kind: kind,
        associatedPropertyName: associatedPropertyName
    };
}

export function fullPath(
    fullPath: string,
    kind: InspectionTokenKind = InspectionTokenKind.Identifier,
    associatedPropertyName: string = "path"
): InspectionToken[] {
    const lastSlash = Math.max(fullPath.lastIndexOf("/"), fullPath.lastIndexOf("\\"));
    const fileNameExt = lastSlash !== -1 ? fullPath.slice(lastSlash + 1) : fullPath;
    const extSeparator = fullPath.lastIndexOf(".");
    const parsedPath = {
        name: extSeparator !== -1 ? fileNameExt.slice(0, extSeparator) : fileNameExt,
        ext: extSeparator !== -1 ? fileNameExt.slice(extSeparator + 1) : undefined
    };

    const tokens: InspectionToken[] = [
        {
            text: parsedPath.name,
            kind: kind,
            hint: fullPath,
            associatedPropertyName: associatedPropertyName,
        },
    ];
    if (parsedPath.ext) {
        tokens.push(
            trivia(".", associatedPropertyName),
            {
                text: parsedPath.ext,
                kind: kind,
                hint: fullPath,
                associatedPropertyName: associatedPropertyName
            }
        );
    }

    return tokens;
}
