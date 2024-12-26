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
    GreaterThanEquals = ">=",
    LessThan = "<",
    LessThanEquals = "<=",
    Contains = "contains",
    Matches = "matches"
}

export const OPERATORS_BY_TYPE: Record<string, FilterBuilderOperation[]> = {
    "string": [
        FilterBuilderOperation.Equals,
        FilterBuilderOperation.NotEquals,
        FilterBuilderOperation.Contains,
        FilterBuilderOperation.Matches,
    ],
    "number": [
        FilterBuilderOperation.Equals,
        FilterBuilderOperation.NotEquals,
        FilterBuilderOperation.GreaterThan,
        FilterBuilderOperation.GreaterThanEquals,
        FilterBuilderOperation.LessThan,
        FilterBuilderOperation.LessThanEquals
    ],
    "boolean": [
        FilterBuilderOperation.Equals,
        FilterBuilderOperation.NotEquals
    ]
}

export interface IFilterBuilderEntry {
    property: string,
    operation: FilterBuilderOperation,
    value: string | number | boolean
}

export type NodeFilter = (node: MultigraphNode) => boolean;

export function buildNodeFilter(
    searchText: string | null,
    mode: SearchMode,
    variableNames?: string[],
    filterBuilder?: IFilterBuilderEntry[]
): NodeFilter | null {

    let filter: NodeFilter | null = null;
    const isTextEmpty = searchText == null || searchText.trim() == "";

    if (isTextEmpty && (filterBuilder == null || filterBuilder.length == 0)) {
        throw new Error("Nodes cannot be searched without either a search text or at least one filter.");
    }

    if (isTextEmpty) {
        filter = (_n) => true;
    }

    if (!isTextEmpty && mode === SearchMode.Contains) {
        let lowerCaseText = searchText.toLowerCase();
        filter = (node: MultigraphNode) => node.name != null
            && node.name.toLowerCase().includes(lowerCaseText);
    }

    if (!isTextEmpty && mode === SearchMode.Regex) {
        let regex = new RegExp(searchText, "i");
        filter = (node: MultigraphNode) => node.name != null && regex.test(node.name);
    }

    if (mode === SearchMode.JavaScript) {
        if (!variableNames || variableNames.length == 0) {
            return null;
        }

        let fn = new Function("n", `let { ${variableNames.join(', ')} } = n; return !!(${searchText});`);
        filter = fn as (node: MultigraphNode) => boolean;
    }

    if (!filter) {
        throw new Error(`'${mode}' is an unknown search mode.`)
    }

    if (filterBuilder) {
        const innerFilter = filter;
        filter = (node: MultigraphNode) => {
            if (!innerFilter(node)) {
                return false;
            }

            for (const entry of filterBuilder) {
                if (entry.value == null || entry.value === "") {
                    throw new Error(`A filter for '${entry.property}' cannot be empty.`);
                }

                if (node[entry.property] === undefined) {
                    return false;
                }

                switch (entry.operation) {
                    case FilterBuilderOperation.Equals:
                        if (typeof node[entry.property] === "string") {
                            if ((<string>node[entry.property]).toLowerCase()
                                !== entry.value.toString().toLowerCase().trim()) {
                                return false;
                            }
                        } else if (node[entry.property] !== entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.NotEquals:
                        if (typeof node[entry.property] === "string") {
                            if ((<string>node[entry.property]).toLowerCase()
                                === entry.value.toString().toLowerCase().trim()) {
                                return false;
                            }
                        } else if (node[entry.property] === entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.Contains:
                        if (typeof node[entry.property] !== "string"
                            || !(<string>node[entry.property]).toLowerCase()
                                .includes(entry.value.toString().toLowerCase().trim())) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.Matches:
                        let regex = new RegExp(entry.value.toString(), "i");
                        if (typeof node[entry.property] !== "string"
                            || !regex.test(node[entry.property])) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.GreaterThan:
                        if (typeof node[entry.property] !== "number"
                            || node[entry.property] <= (<number>entry.value)) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.GreaterThanEquals:
                        if (typeof node[entry.property] !== "number"
                            || node[entry.property] < (<number>entry.value)) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.LessThan:
                        if (typeof node[entry.property] !== "number"
                            || node[entry.property] >= (<number>entry.value)) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.LessThanEquals:
                        if (typeof node[entry.property] !== "number"
                            || node[entry.property] > (<number>entry.value)) {
                            return false;
                        }
                        break;
                    default:
                        throw new Error(`Filter operation '${entry.operation}' is not supported.`)
                }
            }

            return true;
        }
    }

    return filter;
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
