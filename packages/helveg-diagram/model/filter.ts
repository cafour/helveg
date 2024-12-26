import { MultigraphNode, Multigraph } from "./data-model.ts";

export enum SearchMode {
    Contains = "contains",
    Regex = "regex",
    JavaScript = "js"
}

export enum FilterBuilderOperation {
    Equals = "==",
    NotEquals = "!=",
    GreaterThan = ">",
    GreterThanEquals = ">=",
    LessThan = "<",
    LessThanEquals = "<=",
    Contains = "contains",
    Matches = "matches"
}

export interface IFilterBuilderEntry {
    property: string,
    operation: FilterBuilderOperation,
    value: string
}

export type NodeFilter = (node: MultigraphNode) => boolean;

export function buildNodeFilter(
    searchText: string | null,
    mode: SearchMode,
    variableNames?: string[]): NodeFilter | null {

    if (searchText == null || searchText.trim() == "") {
        return null;
    }

    if (mode === SearchMode.Contains) {
        let lowerCaseText = searchText.toLowerCase();
        return (node: MultigraphNode) => node.name != null
            && node.name.toLowerCase().includes(lowerCaseText);
    }

    if (mode === SearchMode.Regex) {
        let regex = new RegExp(searchText, "i");
        return (node: MultigraphNode) => node.name != null && regex.test(node.name);
    }

    if (mode === SearchMode.JavaScript) {
        if (!variableNames || variableNames.length == 0) {
            return null;
        }

        let fn = new Function("n", `let { ${variableNames.join(', ')} } = n; return !!(${searchText});`);
        return fn as (node: MultigraphNode) => boolean;
    }

    throw new Error(`'${mode}' is an unknown search mode.`)
}

export function* filterNodes(graph: Multigraph, filter: NodeFilter | null, invert: boolean = false) {
    if (!filter) {
        return;
    }

    for (let id in graph.nodes) {
        // '!=' works as logical XOR here
        if (filter(graph.nodes[id]) != invert) {
            yield id;
        }
    }
}
