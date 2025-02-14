import { HelvegGraph, HelvegGraphAttributes, HelvegNodeAttributes } from "../model/graph.ts";
import {
    FALLBACK_INSPECTOR,
    identifier,
    Inspection,
    InspectionExpression,
    InspectionToken,
    InspectionTokenKind,
    Inspector,
    keyword,
    MISSING_TEXT as MISSING_TEXT,
    name,
    fullPath,
    SPACE,
    TokenFactory,
    trivia,
    type,
} from "../model/inspect.ts";
import { CSharpNode, EntityKind, MemberAccessibility, TypeKind } from "./model.ts";

export const CSHARP_INSPECTOR: Inspector = (graph, node) => {
    const result: Inspection = FALLBACK_INSPECTOR(graph, node);
    switch (node.kind) {
        case EntityKind.Solution:
        case EntityKind.Project:
            result.expression.tokens = fullPath(node.model!.path, InspectionTokenKind.Identifier, "path");
            break;
        case EntityKind.Namespace:
            result.expression = inspectNamespace(graph, node);
            break;
        case EntityKind.Type:
            result.expression = inspectType(graph, node);
            break;
        case EntityKind.Field:
            result.expression = inspectField(graph, node);
            break;
        case EntityKind.Property:
            result.expression = inspectProperty(graph, node);
            // TODO: Special case: indexers are more like methods
            break;
        case EntityKind.Event:
            result.expression = inspectEvent(graph, node);
            break;
        case EntityKind.Method:
            result.expression = inspectMethod(graph, node);
            // TODO: Special case: constructors, static constructors, destructors
            // TODO: Special case: getters and setters?
            break;
        case EntityKind.Parameter:
            result.expression = inspectParameter(graph, node);
            break;
    }

    return result;
};

function walkBackwards(
    graph: HelvegGraph,
    node: HelvegNodeAttributes,
    separator: string,
    relation: string,
    stopCondition: (node: HelvegNodeAttributes) => boolean,
    tokenFactory: TokenFactory = identifier,
    textGetter = (node: HelvegNodeAttributes) => node.model.name ?? MISSING_TEXT
): InspectionToken[] {
    const tokens: InspectionToken[] = [];

    function appendName(current: HelvegNodeAttributes, depth: number = 0) {
        if (stopCondition(current)) {
            return;
        }

        const parent = graph.source(graph.filterInEdges(current.id, (_e, ea) => ea.relation === relation)[0]);
        if (parent) {
            appendName(graph.getNodeAttributes(parent), depth + 1);
        }

        tokens.push(tokenFactory(textGetter(current)));

        if (depth !== 0) {
            tokens.push({
                kind: InspectionTokenKind.Trivia,
                text: separator,
            });
        }
    }

    appendName(node);
    return tokens;
}

function inspectNamespace(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    if (node.model.name === "global") {
        return {
            tokens: [keyword("global", "isGlobal"), trivia(" "), keyword("namespace", "kind")],
        };
    }

    const expression = {
        tokens: [keyword("namespace", "kind"), trivia(" ")],
    };

    expression.tokens.push(
        ...walkBackwards(
            graph,
            node,
            ".",
            "declares",
            (n) => n.kind != EntityKind.Namespace || n.model.name == "global"
        )
    );

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
    [K in keyof T as string extends K ? never : number extends K ? never : symbol extends K ? never : K]: T[K];
};

function modifier(
    node: HelvegNodeAttributes,
    property: keyof RemoveIndex<CSharpNode> & string,
    keywordName: string,
    appendSpace: boolean = true
): InspectionToken[] {
    if (!node.model[property]) {
        return [];
    }
    const keywordToken = keyword(keywordName, property);
    if (appendSpace) {
        return [keywordToken, SPACE];
    }

    return [keywordToken];
}

function accessibility(node: HelvegNodeAttributes): InspectionToken[] {
    const tokens: InspectionToken[] = [];
    const model = node.model as CSharpNode;
    if (model.accessibility) {
        tokens.push(
            ...ACCESSIBILITY_SYNTAX[model.accessibility].map((t) => {
                return { ...t, associatedPropertyName: "accessibility" };
            })
        );
        tokens.push(SPACE);
    }
    return tokens;
}

function typeName(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionToken[] {
    const tokens = [];
    if (node.model.isNested) {
        tokens.push(...walkBackwards(graph, node, ".", "declares", (n) => n.kind != EntityKind.Type, type));
    } else {
        tokens.push(name(node, InspectionTokenKind.Type));
    }
    return tokens;
}

function typeParameterList(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionToken[] {
    const model = node.model as CSharpNode;
    const tokens: InspectionToken[] = [];
    if (model.arity !== undefined && model.arity > 0) {
        tokens.push(trivia("<"));

        const typeParameters = graph
            .filterOutEdges(
                node.id,
                (e, ea, _s, _t, _sa, ta) => ea.relation === "declares" && ta.model.kind === EntityKind.TypeParameter
            )
            .map((e) => graph.getTargetAttributes(e));
        for (let i = 0; i < typeParameters.length; ++i) {
            const typeParameter = typeParameters[i];

            tokens.push(type(typeParameter.model.name ?? MISSING_TEXT));

            if (i != typeParameters.length - 1) {
                tokens.push(trivia(", "));
            }
        }

        tokens.push(trivia(">"));
    }
    return tokens;
}

function inspectType(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    const model = node.model as CSharpNode;

    expression.tokens.push(...accessibility(node));

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    if (model.typeKind === TypeKind.Class) {
        expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    }
    expression.tokens.push(...modifier(node, "isReadOnly", "readonly"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));

    switch (model.typeKind) {
        case TypeKind.Class:
            if (model.isRecord) {
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

const KEYWORD_TYPES = new Set<string>(["string", "int", "long", "float", "double", "void", "byte", "char", "short"]);

function externalType(fullName: string | null | undefined, associatedPropertyName?: string): InspectionToken {
    fullName ??= MISSING_TEXT;
    const lastSeparatorIndex = fullName.lastIndexOf(".");
    // NB: Doesn't work because of generics. :( Needs a parser.
    // const shortName = lastSeparatorIndex !== -1 ? fullName.slice(lastSeparatorIndex + 1) : fullName;
    return {
        kind: KEYWORD_TYPES.has(fullName) ? InspectionTokenKind.Keyword : InspectionTokenKind.Type,
        text: fullName,
        associatedPropertyName: associatedPropertyName,
        hint: fullName,
    };
}

function inspectField(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    const model = node.model as CSharpNode;

    expression.tokens.push(...accessibility(node));

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isReadOnly", "readonly"));
    expression.tokens.push(...modifier(node, "isVolatile", "volatile"));
    expression.tokens.push(...modifier(node, "isConst", "const"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(model.fieldType, "fieldType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));
    expression.tokens.push(trivia(";"));

    return expression;
}

function inspectProperty(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    const model = node.model as CSharpNode;

    expression.tokens.push(...accessibility(node));

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(model.propertyType, "propertyType"));

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

function inspectEvent(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    const model = node.model as CSharpNode;

    expression.tokens.push(...accessibility(node));

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(model.eventType, "eventType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));
    expression.tokens.push(trivia(";"));

    return expression;
}

function inspectMethod(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    const model = node.model as CSharpNode;

    expression.tokens.push(...accessibility(node));

    expression.tokens.push(...modifier(node, "isStatic", "static"));
    expression.tokens.push(...modifier(node, "isVirtual", "virtual"));
    expression.tokens.push(...modifier(node, "isSealed", "sealed"));
    expression.tokens.push(...modifier(node, "isOverride", "override"));
    expression.tokens.push(...modifier(node, "isAbstract", "abstract"));
    expression.tokens.push(...modifier(node, "isExtern", "extern"));
    expression.tokens.push(...modifier(node, "isAsync", "async"));
    expression.tokens.push(...modifier(node, "isPartial", "partial"));

    // TODO: look up the type through typeof instead of relying on the hint
    expression.tokens.push(externalType(model.returnType, "returnType"));

    expression.tokens.push(trivia(" "));

    expression.tokens.push(name(node));

    expression.tokens.push(...typeParameterList(graph, node));

    expression.tokens.push(trivia("("));

    if (model.parameterCount !== undefined && model.parameterCount > 0) {
        const parameters = graph
            .filterOutEdges(
                node.id,
                (e, ea, _s, _t, _sa, ta) => ea.relation === "declares" && ta.model.kind === EntityKind.Parameter
            )
            .map((e) => graph.getTargetAttributes(e));

        for (let i = 0; i < parameters.length; ++i) {
            const parameter = parameters[i];

            expression.tokens.push(externalType(parameter.model.parameterType));
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

function inspectParameter(graph: HelvegGraph, node: HelvegNodeAttributes): InspectionExpression {
    const expression: InspectionExpression = {
        tokens: [],
    };

    expression.tokens.push(externalType(node.model.parameterType, "parameterType"));
    expression.tokens.push(trivia(" "));
    expression.tokens.push(name(node));

    return expression;
}
