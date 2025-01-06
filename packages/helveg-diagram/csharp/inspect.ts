import { MULTIGRAPH_NODE_KEY } from "../global.ts";
import { Multigraph } from "../model/data-model.ts";
import { FALLBACK_INSPECTOR, Inspection, InspectionExpression, InspectionTokenKind, Inspector } from "../model/inspect.ts";
import { CSharpNode, EntityKind } from "./model.ts";

export const CSHARP_INSPECTOR: Inspector = (graph, node) => {
    const result: Inspection = FALLBACK_INSPECTOR(graph, node);
    switch (node.kind) {
        case EntityKind.Namespace:
            result.expression = inspectNamespace(graph, node as CSharpNode);
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
