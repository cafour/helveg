import { type Readable, readable, type Writable, writable } from "svelte/store";
import type {
    DataModel,
    Diagram,
    DiagramStats,
    DiagramStatus,
    HelvegGraph,
    ILogger,
} from "./deps/helveg-diagram";
import {
    DEFAULT_DONUT_PROGRAM_OPTIONS,
    type HelvegEvent,
    sublogger,
} from "./deps/helveg-diagram";
import * as Options from "./options.ts";
import { OperationExecutor } from "./operations/executor.ts";
import { AppTools } from "./const.ts";
import { createDefaultExecutor } from "./operations/index.ts";

export interface IExplorerState {
    rootElement: HTMLElement;
    diagram: Diagram;
    model: Readable<DataModel>;
    graph: Readable<HelvegGraph | undefined>;
    status: Readable<DiagramStatus>;
    stats: Readable<DiagramStats>;
    logger: ILogger;
    operationExecutor: OperationExecutor;
    selectedNode: Writable<string | null>;

    dataOptions: Writable<Options.DataOptions>;
    layoutOptions: Writable<Options.LayoutOptions>;
    appearanceOptions: Writable<Options.AppearanceOptions>;
    exportOptions: Writable<Options.ExportOptions>;
    toolOptions: Writable<Options.ToolOptions>;
}

function wrapVariable<T>(
    get: () => T,
    set: (value: T) => void,
    event: HelvegEvent<T>,
): Writable<T> {
    const store: Writable<T> = {
        set,
        update(updater) {
            set(updater(get()));
        },
        subscribe(run) {
            event.subscribe(run);
            run(get());
            return () => event.unsubscribe(run);
        },
    };
    return store;
}

export function createExplorerState(
    rootElement: HTMLElement,
    diagram: Diagram,
): IExplorerState {
    const model = readable(diagram.model, (set) => {
        diagram.events.modelChanged.subscribe(set);
        return () => diagram.events.modelChanged.unsubscribe(set);
    });

    const graph = readable<HelvegGraph | undefined>(diagram.graph, (set) => {
        diagram.events.graphChanged.subscribe(set);
        return () => diagram.events.graphChanged.unsubscribe(set);
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

    const selectedNode = wrapVariable(
        () => diagram.selectedNode,
        (value) => diagram.selectedNode = value,
        diagram.events.nodeSelected,
    );

    const dataOptions = createOptions<Options.DataOptions>(
        "data",
        {
            ...structuredClone(Options.DEFAULT_DATA_OPTIONS),
            ...diagram.options.refresh,
        },
    );

    const layoutOptions = createOptions<Options.LayoutOptions>(
        "layout",
        structuredClone(Options.DEFAULT_LAYOUT_OPTIONS),
    );
    layoutOptions.update((o) => {
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
        },
    );
    appearanceOptions.subscribe((o) => {
        diagram.relationStylistParams = o.relationColors;
        diagram.nodeStylistParams = o.nodeColorSchema;

        const glyphOptions = { ...diagram.glyphProgramOptions };
        glyphOptions.isFireAnimated = o.glyph.isFireAnimated;
        glyphOptions.showFire = o.glyph.showFire;
        glyphOptions.showIcons = o.glyph.showIcons;
        glyphOptions.showLabels = o.glyph.showLabels;
        glyphOptions.glyphShape = o.glyph.glyphShape;
        glyphOptions.showDiffs = o.glyph.showDiffs;
        glyphOptions.showCollapsedNodeIndicators =
            o.glyph.showCollapsedNodeIndicators;
        glyphOptions.sizingMode = o.glyph.sizingMode;
        glyphOptions.hatchingWidth = o.glyph.showHatching
            ? DEFAULT_DONUT_PROGRAM_OPTIONS.hatchingWidth
            : 0;
        glyphOptions.showContours = o.glyph.showContours;

        glyphOptions.crustWidth = o.codePizza.crustWidth;
        glyphOptions.sauceWidth = o.codePizza.sauceWidth;
        glyphOptions.isPizzaEnabled = o.codePizza.isEnabled;
        glyphOptions.pizzaToppings = o.codePizza.pizzaToppings ??
            glyphOptions.pizzaToppings;
        diagram.glyphProgramOptions = glyphOptions;
    });

    const exportOptions = createOptions<Options.ExportOptions>(
        "export",
        structuredClone(Options.DEFAULT_EXPORT_OPTIONS),
    );
    const toolOptions = createOptions<Options.ToolOptions>(
        "tool",
        structuredClone(Options.DEFAULT_TOOL_OPTIONS),
    );
    toolOptions.subscribe(o => {
        diagram.shouldFixateSelectedNode = o.showProperties.shouldFixateSelectedNode;
    })

    const state: IExplorerState = {
        rootElement,
        diagram,
        model,
        graph,
        logger,
        status,
        stats,
        operationExecutor: null!,
        selectedNode,

        dataOptions,
        layoutOptions,
        appearanceOptions,
        exportOptions,
        toolOptions,
    };

    state.operationExecutor = createDefaultExecutor(state);
    return state;
}

function createOptions<T>(
    storageName: string,
    defaults: T,
): Writable<T> {
    const options = writable({
        ...defaults,
        ...Options.loadOptions<T>(storageName),
    });
    options.subscribe((v) => Options.saveOptions<T>(storageName, v));
    return options;
}
