import type { Readable, Writable } from "svelte/store";
import type { DataModel, Diagram, DiagramStats, DiagramStatus, ILogger } from "./deps/helveg-diagram";
import * as Options from "./options.ts";
import type { OperationExecutor } from "./operation-executor.ts";

export interface IExplorerState {
    diagram: Diagram,
    model: Readable<DataModel>,
    status: Readable<DiagramStatus>,
    stats: Readable<DiagramStats>,
    logger: ILogger,
    operationExecutor: OperationExecutor,
    mouseOperation: Writable<string>,
    selectedNode: Writable<string | undefined>,

    dataOptions: Writable<Options.DataOptions>,
    layoutOptions: Writable<Options.LayoutOptions>,
    appearanceOptions: Writable<Options.AppearanceOptions>,
    exportOptions: Writable<Options.ExportOptions>,
    toolOptions: Writable<Options.ToolOptions>,
}

