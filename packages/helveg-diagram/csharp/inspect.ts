import { MULTIGRAPH_NODE_KEY } from "../global.ts";
import { Multigraph } from "../model/data-model.ts";
import { FALLBACK_INSPECTOR, identifier, Inspection, InspectionExpression, InspectionToken, InspectionTokenKind, Inspector, keyword, MISSING_TEXT as MISSING_TEXT, name, SPACE, TokenFactory, trivia, type } from "../model/inspect.ts";
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
        case EntityKind.Property:
            result.expression = inspectProperty(graph, node as CSharpNode);
            // TODO: Special case: indexers are more like methods
            break;
        case EntityKind.Event:
            result.expression = inspectEvent(graph, node as CSharpNode);
            break;
        case EntityKind.Method:
            result.expression = inspectMethod(graph, node as CSharpNode);
            break;
        case EntityKind.Parameter:
            result.expression = inspectParameter(graph, node as CSharpNode);
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
    textGetter = (node: CSharpNode) => node.name ?? MISSING_TEXT,
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

function typeParameterList(graph: Multigraph, node: CSharpNode): InspectionToken[] {
    const tokens: InspectionToken[] = [];
    if (node.arity !== undefined && node.arity > 0) {
        tokens.push(trivia("<"));

        const typeParameters = Object.values(graph.relations["declares"].edges ?? {})
            .filter(e => e.src === node[MULTIGRAPH_NODE_KEY] && graph.nodes[e.dst!].kind === EntityKind.TypeParameter)
            .map(e => graph.nodes[e.dst!] as CSharpNode);
        for (let i = 0; i < typeParameters.length; ++i) {
            const typeParameter = typeParameters[i];

            tokens.push(type(typeParameter.name ?? MISSING_TEXT));

            if (i != typeParameters.length - 1) {
                tokens.push(trivia(", "));
            }
        }

        tokens.push(trivia(">"));
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

    expression.tokens.push(...typeParameterList(graph, node));

    return expression;
}

function externalType(fullName: string | null | undefined, associatedPropertyName?: string): InspectionToken {
    fullName ??= MISSING_TEXT;
    const lastSeparatorIndex = fullName.lastIndexOf(".");
    // NB: Doesn't work because of generics. :( Needs a parser.
    // const shortName = lastSeparatorIndex !== -1 ? fullName.slice(lastSeparatorIndex + 1) : fullName;
    return {
        kind: InspectionTokenKind.Type,
        text: fullName,
        associatedPropertyName: associatedPropertyName,
        hint: fullName
    };
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
    expression.tokens.push(externalType(node.fieldType, "fieldType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));
    expression.tokens.push(trivia(";"));

    return expression;
}

function inspectProperty(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    if (node.accessibility) {
        expression.tokens.push(...ACCESSIBILITY_SYNTAX[node.accessibility]
            .map(t => { return { ...t, associatedPropertyName: "accessibility" }; }));
        expression.tokens.push(SPACE);
    }

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(node.propertyType, "propertyType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));

    expression.tokens.push(trivia(" { "));
    if (node.isWriteOnly !== true) {
        expression.tokens.push(keyword("get"));
        expression.tokens.push(trivia("; "));
    }
    if (node.isReadOnly !== true) {
        expression.tokens.push(keyword("set"));
        expression.tokens.push(trivia("; "));
    }
    expression.tokens.push(trivia("}"));

    return expression;
}

function inspectEvent(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    if (node.accessibility) {
        expression.tokens.push(...ACCESSIBILITY_SYNTAX[node.accessibility]
            .map(t => { return { ...t, associatedPropertyName: "accessibility" }; }));
        expression.tokens.push(SPACE);
    }

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(node.eventType, "eventType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));
    expression.tokens.push(trivia(";"));

    return expression;
}

function inspectMethod(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    if (node.accessibility) {
        expression.tokens.push(...ACCESSIBILITY_SYNTAX[node.accessibility]
            .map(t => { return { ...t, associatedPropertyName: "accessibility" }; }));
        expression.tokens.push(SPACE);
    }

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));
    expression.tokens.push(...modifier(node, "isAsync", "async"));
    expression.tokens.push(...modifier(node, "isPartial", "partial"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(node.returnType, "returnType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));

    expression.tokens.push(...typeParameterList(graph, node));

    expression.tokens.push(trivia("("));

    if (node.parameterCount !== undefined && node.parameterCount > 0) {
        const parameters = Object.values(graph.relations["declares"].edges ?? {})
            .filter(e => e.src === node[MULTIGRAPH_NODE_KEY] && graph.nodes[e.dst!].kind === EntityKind.Parameter)
            .map(e => graph.nodes[e.dst!] as CSharpNode);

        for (let i = 0; i < parameters.length; ++i) {
            const parameter = parameters[i];

            expression.tokens.push(externalType(parameter.parameterType));
            expression.tokens.push(trivia(" "));
            expression.tokens.push(name(parameter));

            if (i != parameters.length - 1) {
                expression.tokens.push(trivia(", "));
            }
        }
    }

    expression.tokens.push(trivia(")"));

    return expression;
}


function inspectParameter(graph: Multigraph, node: CSharpNode): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: []
    };

    expression.tokens.push(externalType(node.parameterType, "parameterType"));
    expression.tokens.push(trivia(" "));
    expression.tokens.push(name(node));

    return expression;
}
