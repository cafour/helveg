import { MULTIGRAPH_NODE_KEY } from "../global.ts";
import { Multigraph } from "../model/data-model.ts";
import { FALLBACK_INSPECTOR, Inspection, InspectionExpression, InspectionToken, InspectionTokenKind, Inspector, keyword, SPACE, trivia, type } from "../model/inspect.ts";
import { CSharpNode, EntityKind, MemberAccessibility, TypeKind } from "./model.ts";

export const CSHARP_INSPECTOR: Inspector = (graph, node) => {
    const result: Inspection = FALLBACK_INSPECTOR(graph, node);
    switch (node.kind) {
        case EntityKind.Namespace:
            result.expression = inspectNamespace(graph, node as CSharpNode);
            break;
        case EntityKind.Type:
            result.expression = inspectType(graph, node as CSharpNode);
            break;
    }

    return result;
};

function inspectNamespace(graph: Multigraph, node: CSharpNode): InspectionExpression {
    if (node.name === "global") {
        return {
            tokens: [
                {
                    text: "global",
                    kind: InspectionTokenKind.Keyword
                },
                {
                    text: " ",
                    kind: InspectionTokenKind.Trivia
                },
                {
                    text: "namespace",
                    kind: InspectionTokenKind.Keyword
                },
            ]
        }
    }

    const expression = {
        tokens: [
            {
                text: "namespace",
                kind: InspectionTokenKind.Keyword
            },
            {
                text: " ",
                kind: InspectionTokenKind.Trivia
            }
        ]
    };

    function appendNamespaceName(current: CSharpNode, depth: number = 0) {
        if (current.name === "global") {
            return;
        }

        const parent = Object.values(graph.relations["declares"].edges ?? {})
            .filter(e => e.dst === current[MULTIGRAPH_NODE_KEY])[0]?.src;
        if (parent) {
            appendNamespaceName(graph.nodes[parent] as CSharpNode, depth + 1);
        }

        expression.tokens.push({
            kind: InspectionTokenKind.Identifier,
            text: current.name ?? "<missing>"
        });

        if (depth !== 0) {
            expression.tokens.push({
                kind: InspectionTokenKind.Trivia,
                text: "."
            });
        }

    }
    appendNamespaceName(node);

    return expression;
}

const ACCESSIBILITY_SYNTAX: Record<MemberAccessibility, readonly InspectionToken[]> = {
    Invalid: [keyword("<invalid>")],
    Internal: [keyword("internal")],
    Private: [keyword("private")],
    Protected: [keyword("protected")],
    ProtectedAndInternal: [keyword("private"), SPACE, keyword("protected")],
    ProtectedOrInternal: [keyword("protected"), SPACE, keyword("internal")],
    Public: [keyword("public")],
};

type RemoveIndex<T> = {
    [K in keyof T as
    string extends K
    ? never
    : number extends K
    ? never
    : symbol extends K
    ? never
    : K
    ]: T[K];
};

function modifier(
    node: CSharpNode,
    property: keyof RemoveIndex<CSharpNode> & string,
    keywordName: string,
    appendSpace: boolean = true
): InspectionToken[] {
    if (!node[property]) {
        return [];
    }
    const keywordToken = keyword(keywordName, property);
    if (appendSpace) {
        return [keywordToken, SPACE];
    }

    return [keywordToken];
}

function inspectType(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    if (node.accessibility) {
        expression.tokens.push(...ACCESSIBILITY_SYNTAX[node.accessibility]
            .map(t => { return { ...t, associatedPropertyName: "accessibility" }; }));
        expression.tokens.push(SPACE);
    }

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    if (node.typeKind === TypeKind.Class) {
        expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    }
    expression.tokens.push(...modifier(node, "isReadOnly", "readonly"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    
    switch(node.typeKind) {
        case TypeKind.Class:
            if (node.isRecord) {
                expression.tokens.push(keyword("record", "isRecord"));
            } else {
                expression.tokens.push(keyword("class", "typeKind"));
            }
            expression.tokens.push(SPACE);
            break;
        case TypeKind.Struct:
            expression.tokens.push(...modifier(node, "isRecord", "record"));
            expression.tokens.push(keyword("struct", "typeKind"), SPACE);
            break;
        case TypeKind.Enum:
            expression.tokens.push(keyword("enum", "typeKind"), SPACE);
            break;
        case TypeKind.Delegate:
            expression.tokens.push(keyword("delegate", "typeKind"), SPACE);
            break;
        case TypeKind.Interface:
            expression.tokens.push(keyword("interface", "typeKind"), SPACE);
            break;
    }

    expression.tokens.push(type(node.name ?? "<unknown>", "name"));
    return expression;
}
