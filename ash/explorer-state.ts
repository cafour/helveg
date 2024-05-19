import { readable, writable, type Readable, type Writable } from "svelte/store";
import type { DataModel, Diagram, DiagramStats, DiagramStatus, ILogger } from "./deps/helveg-diagram";
import { createCsharpRelationStylist, sublogger } from "./deps/helveg-diagram";
import * as Options from "./options.ts";
import { OperationExecutor } from "./operation-executor.ts";

export interface IExplorerState {
    rootElement: HTMLElement,
    diagram: Diagram,
    model: Readable<DataModel>,
    status: Readable<DiagramStatus>,
    stats: Readable<DiagramStats>,
    logger: ILogger,
    operationExecutor: OperationExecutor,
    selectedTool: Writable<string>,
    selectedNode: Writable<string | null>,

    dataOptions: Writable<Options.DataOptions>,
    layoutOptions: Writable<Options.LayoutOptions>,
    appearanceOptions: Writable<Options.AppearanceOptions>,
    exportOptions: Writable<Options.ExportOptions>,
    toolOptions: Writable<Options.ToolOptions>,
}

export function createExplorerState(rootElement: HTMLElement, diagram: Diagram): IExplorerState {
    const model = readable(diagram.model, (set) => {
        diagram.events.modelChanged.subscribe(set);
        return () => diagram.events.modelChanged.unsubscribe(set);
    });

    const logger = sublogger(diagram.logger, "explorer");

    const status = readable(diagram.status, (set) => {
        diagram.events.statusChanged.subscribe(set);
        return () => diagram.events.statusChanged.unsubscribe(set);
    });

    const stats = readable(diagram.stats, (set) => {
        diagram.events.statsChanged.subscribe(set);
        return () => diagram.events.statsChanged.unsubscribe(set);
    });

    const selectedTool = writable<string>("show-properties");

    const selectedNode = writable<string | null>(null);

    const dataOptions = createOptions<Options.DataOptions>(
        "data",
        {
            ...structuredClone(Options.DEFAULT_DATA_OPTIONS),
            ...diagram.options.refresh,
        }
    );

    const layoutOptions = createOptions<Options.LayoutOptions>(
        "layout",
        structuredClone(Options.DEFAULT_LAYOUT_OPTIONS)
    );
    layoutOptions.update(o => {
        o.tidyTree.relation = diagram.mainRelation;
        return o;
    });
    layoutOptions.subscribe((v) => {
        diagram.mainRelation = v.tidyTree.relation;
        diagram.forceAtlas2Options = v.forceAtlas2;
    });

    const appearanceOptions = createOptions<Options.AppearanceOptions>(
        "appearance",
        {
            relationColors: {},
            ...structuredClone(Options.DEFAULT_APPEARANCE_OPTIONS),
        }
    );
    appearanceOptions.subscribe(o => {
        diagram.relationStylist = createCsharpRelationStylist(
            o.relationColors!
        );
    });

    const exportOptions = createOptions<Options.ExportOptions>(
        "export",
        structuredClone(Options.DEFAULT_EXPORT_OPTIONS)
    );
    const toolOptions = createOptions<Options.ToolOptions>(
        "tool",
        structuredClone(Options.DEFAULT_TOOL_OPTIONS)
    );

    const state: IExplorerState = {
        rootElement,
        diagram,
        model,
        logger,
        status,
        stats,
        operationExecutor: null!,
        selectedTool,
        selectedNode,

        dataOptions,
        layoutOptions,
        appearanceOptions,
        exportOptions,
        toolOptions
    };

    state.operationExecutor = new OperationExecutor(state);
    return state;
}

function createOptions<T>(
    storageName: string,
    defaults: T
): Writable<T> {
    const options = writable({
        ...defaults,
        ...Options.loadOptions<T>(storageName),
    });
    options.subscribe((v) => Options.saveOptions<T>(storageName, v));
    return options;
}
