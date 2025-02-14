import { MultigraphNode, Multigraph } from "./data-model.ts";
import { HelvegGraph, HelvegNodeAttributes } from "./graph.ts";

export enum SearchMode {
    Contains = "contains",
    Regex = "regex",
    JavaScript = "js",
}

export enum FilterBuilderOperation {
    Equals = "==",
    NotEquals = "!=",
    GreaterThan = ">",
    GreaterThanEquals = ">=",
    LessThan = "<",
    LessThanEquals = "<=",
    Contains = "contains",
    Matches = "matches",
}

export const OPERATORS_BY_TYPE: Record<string, FilterBuilderOperation[]> = {
    string: [
        FilterBuilderOperation.Equals,
        FilterBuilderOperation.NotEquals,
        FilterBuilderOperation.Contains,
        FilterBuilderOperation.Matches,
    ],
    number: [
        FilterBuilderOperation.Equals,
        FilterBuilderOperation.NotEquals,
        FilterBuilderOperation.GreaterThan,
        FilterBuilderOperation.GreaterThanEquals,
        FilterBuilderOperation.LessThan,
        FilterBuilderOperation.LessThanEquals,
    ],
    boolean: [FilterBuilderOperation.Equals, FilterBuilderOperation.NotEquals],
};

export interface IFilterBuilderEntry {
    property: string;
    operation: FilterBuilderOperation;
    value: string | number | boolean;
}

export type NodeFilter = (node: HelvegNodeAttributes) => boolean;

export function buildNodeFilter(
    searchText: string | null,
    mode: SearchMode,
    variableNames?: string[],
    filterBuilder?: IFilterBuilderEntry[]
): NodeFilter | null {
    searchText?.trim();

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
        filter = (node: HelvegNodeAttributes) =>
            node.model.name != null && node.model.name.toLowerCase().includes(lowerCaseText);
    }

    if (!isTextEmpty && mode === SearchMode.Regex) {
        let regex = new RegExp(searchText, "i");
        filter = (node: HelvegNodeAttributes) => node.model.name != null && regex.test(node.model.name);
    }

    if (!isTextEmpty && mode === SearchMode.JavaScript) {
        if (!variableNames || variableNames.length == 0) {
            return null;
        }

        let fn = new Function("n", `let { ${variableNames.join(", ")} } = n.model; return !!(${searchText});`);
        filter = fn as (node: HelvegNodeAttributes) => boolean;
    }

    if (!filter) {
        throw new Error(`'${mode}' is an unknown search mode.`);
    }

    if (filterBuilder) {
        const innerFilter = filter;
        filter = (node: HelvegNodeAttributes) => {
            if (!innerFilter(node)) {
                return false;
            }

            for (const entry of filterBuilder) {
                if (entry.value == null || entry.value === "") {
                    throw new Error(`A filter for '${entry.property}' cannot be empty.`);
                }

                const propertyValue = node.model[entry.property];

                if (propertyValue === undefined) {
                    return false;
                }

                switch (entry.operation) {
                    case FilterBuilderOperation.Equals:
                        if (typeof propertyValue === "string") {
                            if ((<string>propertyValue).toLowerCase() !== entry.value.toString().toLowerCase().trim()) {
                                return false;
                            }
                        } else if (propertyValue !== entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.NotEquals:
                        if (typeof propertyValue === "string") {
                            if ((<string>propertyValue).toLowerCase() === entry.value.toString().toLowerCase().trim()) {
                                return false;
                            }
                        } else if (propertyValue === entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.Contains:
                        if (
                            typeof propertyValue !== "string" ||
                            !(<string>propertyValue).toLowerCase().includes(entry.value.toString().toLowerCase().trim())
                        ) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.Matches:
                        let regex = new RegExp(entry.value.toString(), "i");
                        if (typeof propertyValue !== "string" || !regex.test(propertyValue)) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.GreaterThan:
                        if (typeof propertyValue !== "number" || propertyValue <= <number>entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.GreaterThanEquals:
                        if (typeof propertyValue !== "number" || propertyValue < <number>entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.LessThan:
                        if (typeof propertyValue !== "number" || propertyValue >= <number>entry.value) {
                            return false;
                        }
                        break;
                    case FilterBuilderOperation.LessThanEquals:
                        if (typeof propertyValue !== "number" || propertyValue > <number>entry.value) {
                            return false;
                        }
                        break;
                    default:
                        throw new Error(`Filter operation '${entry.operation}' is not supported.`);
                }
            }

            return true;
        };
    }

    return filter;
}

export function filterNodes(graph: HelvegGraph, filter: NodeFilter | null, invert: boolean = false) {
    if (!filter) {
        return [];
    }

    return graph.filterNodes((_n, na) => filter(na) != invert);
}
