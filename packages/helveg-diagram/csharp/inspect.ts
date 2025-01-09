import { MULTIGRAPH_NODE_KEY } from "../global.ts";
import { Multigraph } from "../model/data-model.ts";
import { FALLBACK_INSPECTOR, identifier, Inspection, InspectionExpression, InspectionToken, InspectionTokenKind, Inspector, keyword, MISSING_NAME, name, SPACE, TokenFactory, trivia, type } from "../model/inspect.ts";
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
        case EntityKind.Field:
            result.expression = inspectField(graph, node as CSharpNode);
            break;
    }

    return result;
};

function walkBackwards(
    graph: Multigraph,
    node: CSharpNode,
    separator: string,
    relation: string,
    stopCondition: (node: CSharpNode) => boolean,
    tokenFactory: TokenFactory = identifier,
    textGetter = (node: CSharpNode) => node.name ?? MISSING_NAME,
): InspectionToken[] {
    const tokens: InspectionToken[] = [];

    function appendName(current: CSharpNode, depth: number = 0) {
        if (stopCondition(current)) {
            return;
        }

        const parent = Object.values(graph.relations[relation].edges ?? {})
            .filter(e => e.dst === current[MULTIGRAPH_NODE_KEY])[0]?.src;
        if (parent) {
            appendName(graph.nodes[parent] as CSharpNode, depth + 1);
        }

        tokens.push(tokenFactory(textGetter(current)));

        if (depth !== 0) {
            tokens.push({
                kind: InspectionTokenKind.Trivia,
                text: separator
            });
        }

    }

    appendName(node);
    return tokens;
}

function inspectNamespace(graph: Multigraph, node: CSharpNode): InspectionExpression {
    if (node.name === "global") {
        return {
            tokens: [keyword("global", "isGlobal"), trivia(" "), keyword("namespace", "kind")]
        }
    }

    const expression = {
        tokens: [keyword("namespace", "kind"), trivia(" ")]
    };

    expression.tokens.push(
        ...walkBackwards(graph, node, ".", "declares", n => n.kind != EntityKind.Namespace || n.name == "global"));

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

function typeName(graph: Multigraph, node: CSharpNode): InspectionToken[] {
    const tokens = [];
    if (node.isNested) {
        tokens.push(...walkBackwards(
            graph,
            node,
            ".",
            "declares",
            n => n.kind != EntityKind.Type,
            type
        ));
    } else {
        tokens.push(name(node, InspectionTokenKind.Type));
    }
    return tokens;
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

    switch (node.typeKind) {
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

    expression.tokens.push(...typeName(graph, node));

    if (node.arity !== undefined && node.arity > 0) {
        expression.tokens.push(trivia("<"));

        const typeParameters = Object.values(graph.relations["declares"].edges ?? {})
            .filter(e => e.src === node[MULTIGRAPH_NODE_KEY] && graph.nodes[e.dst!].kind === EntityKind.TypeParameter)
            .map(e => graph.nodes[e.dst!] as CSharpNode);
        for (let i = 0; i < typeParameters.length; ++i) {
            const typeParameter = typeParameters[i];

            expression.tokens.push(type(typeParameter.name ?? MISSING_NAME));

            if (i != typeParameters.length - 1) {
                expression.tokens.push(trivia(", "));
            }
        }

        expression.tokens.push(trivia(">"));
    }
    return expression;
}

function inspectField(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    if (node.accessibility) {
        expression.tokens.push(...ACCESSIBILITY_SYNTAX[node.accessibility]
            .map(t => { return { ...t, associatedPropertyName: "accessibility" }; }));
        expression.tokens.push(SPACE);
    }

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isReadOnly", "readonly"));
    expression.tokens.push(...modifier(node, "isVolatile", "volatile"));
    expression.tokens.push(...modifier(node, "isConst", "const"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(type(node.fieldType ?? MISSING_NAME, "fieldType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));
    expression.tokens.push(trivia(";"));

    return expression;
}
