import type { GraphNode } from "./multigraph";
import { SearchMode } from "./options";

export function buildFilter(
    searchText: string | null,
    mode: SearchMode,
    variableNames?: string[]): ((node: GraphNode) => boolean) | null {

    if (searchText == null || searchText.trim() == "") {
        return null;
    }

    if (mode === SearchMode.Contains) {
        let lowerCaseText = searchText.toLowerCase();
        return (node: GraphNode) => node.label != null && node.label.toLowerCase().includes(lowerCaseText);
    }

    if (mode === SearchMode.Regex) {
        let regex = new RegExp(searchText, "i");
        return (node: GraphNode) => node.label != null && regex.test(node.label);
    }

    if (mode === SearchMode.JavaScript) {
        if (!variableNames || variableNames.length == 0) {
            return null;
        }

        let fn = new Function("n", `let { ${variableNames.join(', ')} } = n.properties; return !!(${searchText});`);
        return fn as (node: GraphNode) => boolean;
    }

    throw new Error(`'${mode}' is an unknown search mode.`)
}
