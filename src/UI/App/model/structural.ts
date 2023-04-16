import type { GraphNode } from "./multigraph";
import { DataOptions } from "./options";

export class StructuralState
{
    selectedNode: GraphNode | null = null;
    dataOptions : DataOptions = new DataOptions();
}
