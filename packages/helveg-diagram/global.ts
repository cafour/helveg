import { csharpEdgeStylist, csharpNodeStylist } from "./csharp/style.ts";
import { IconRegistry } from "./model/icons.ts";
import { DEFAULT_ICON_ATLAS_OPTIONS } from "./rendering/iconAtlas.ts"
import { LogSeverity, consoleLogger } from "./model/logger.ts";
import { EdgeStylist, NodeStylist } from "./model/style.ts";
import { IconAtlas } from "./rendering/iconAtlas.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS } from "./rendering/node.glyph.ts";
import { loadJsonScripts, requireJsonScript } from "./model/data.ts";
import { Diagram, DiagramRefreshOptions } from "./diagram/diagram.ts";
import { EMPTY_DATA_MODEL } from "./model/const.ts";
import { IconSetModel } from "./model/icon-set-model.ts";
import { DataModel } from "./model/data-model.ts";

// TODO: be a little bit more selective about what to export
export * from "./model/const.ts";
export * from "./model/data.ts";
export * from "./model/filter.ts";
export * from "./model/graph.ts"
export * from "./model/icons.ts";
export * from "./model/logger.ts";
export * from "./model/style.ts";
export * from "./model/traversal.ts";
export * from "./rendering/export.ts";
export * from "./diagram/diagram.ts";
export * from "./model/data-model.ts";
export * from "./model/icon-set-model.ts";

export interface CreateDiagramOptions {
    element: HTMLElement | null,
    iconSets: IconSetModel[],
    model: DataModel,
    logLevel: LogSeverity,
    nodeStylist: NodeStylist,
    edgeStylist: EdgeStylist,
    mainRelation: string | null,
    iconSize: number,
    refresh: DiagramRefreshOptions
}

export const DEFAULT_CREATE_DIAGRAM_OPTIONS: CreateDiagramOptions = {
    logLevel: LogSeverity.Info,
    model: EMPTY_DATA_MODEL,
    iconSets: [],
    element: null,
    nodeStylist: csharpNodeStylist,
    edgeStylist: csharpEdgeStylist,
    mainRelation: null,
    iconSize: DEFAULT_ICON_ATLAS_OPTIONS.iconSize,
    refresh: {}
};

export function createDiagram(options?: Partial<CreateDiagramOptions>): Diagram {
    const opts = { ...DEFAULT_CREATE_DIAGRAM_OPTIONS, ...options };
    if (!opts.element) {
        opts.element = document.getElementById("helveg");
        if (!opts.element) {
            throw new Error("No explicit element provided and #helveg cannot be found.");
        }
    }

    const iconRegistry = new IconRegistry(consoleLogger("iconRegistry", opts.logLevel));
    opts.iconSets?.forEach(s => iconRegistry.register(s));

    const diagram = new Diagram(opts.element, {
        logLevel: opts.logLevel,
        nodeStylist: opts.nodeStylist,
        edgeStylist: opts.edgeStylist,
        mainRelation: opts.mainRelation ?? Object.keys(opts.model.data?.relations ?? {}).sort()[0] ?? null,
        iconRegistry: iconRegistry,
        glyphProgram: {
            ...DEFAULT_GLYPH_PROGRAM_OPTIONS,
            iconAtlas: new IconAtlas(iconRegistry, { iconSize: opts.iconSize })
        },
        refresh: options?.refresh
    })
    diagram.model = opts.model;
    return diagram;
}

export function loadIconSet(element: Element): Promise<IconSetModel> {
    return requireJsonScript(element);
}

export async function loadIconSets(selector: string): Promise<IconSetModel[]> {
    const scripts = document.querySelectorAll(selector);
    const results = [];
    for (const script of scripts) {
        results.push(await loadIconSet(script));
    }
    return results;
}

export function loadModel(element: Element): Promise<DataModel> {
    return requireJsonScript(element);
}

