import { csharpEdgeStylist, csharpNodeStylist } from "./csharp/style.ts";
import { IconRegistry, IconSet } from "./model/icons.ts";
import { DEFAULT_ICON_ATLAS_OPTIONS } from "./rendering/iconAtlas.ts"
import { LogSeverity, consoleLogger } from "./model/logger.ts";
import { EdgeStylist, NodeStylist } from "./model/style.ts";
import { IconAtlas } from "./rendering/iconAtlas.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS } from "./rendering/node.glyph.ts";
import { loadJsonScripts, loadScript, requireJsonScript } from "./model/data.ts";
import { Diagram } from "./diagram/diagram.ts";
import * as dataModel from "./model/data-model.ts";
import * as iconSetModel from "./model/icon-set-model.ts";
import { EMPTY_DATA_MODEL } from "./model/const.ts";

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
export { dataModel, iconSetModel };

export interface CreateDiagramOptions {
    element: HTMLElement | null,
    iconSets: IconSet[],
    model: dataModel.DataModel,
    logLevel: LogSeverity,
    nodeStylist: NodeStylist,
    edgeStylist: EdgeStylist,
    mainRelation: string | null,
    iconSize: number
}

export const DEFAULT_CREATE_DIAGRAM_OPTIONS: CreateDiagramOptions = {
    logLevel: LogSeverity.Info,
    model: EMPTY_DATA_MODEL,
    iconSets: [],
    element: null,
    nodeStylist: csharpNodeStylist,
    edgeStylist: csharpEdgeStylist,
    mainRelation: null,
    iconSize: DEFAULT_ICON_ATLAS_OPTIONS.iconSize
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
        mainRelation: opts.mainRelation ?? Object.keys(opts.model.multigraph.relations).sort()[0] ?? null,
        iconRegistry: iconRegistry,
        glyphProgram: {
            ...DEFAULT_GLYPH_PROGRAM_OPTIONS,
            iconAtlas: new IconAtlas(iconRegistry, { iconSize: opts.iconSize })
        }
    })
    diagram.model = opts.model;
    return diagram;
}

export async function loadIconSet(element: Element): Promise<iconSetModel.IconSetModel> {
    const text = await loadScript(element);
    return iconSetModel.Convert.toIconSetModel(text);
}

export async function loadIconSets(selector: string): Promise<IconSet[]> {
    const scripts = document.querySelectorAll(selector);
    const results = [];
    for (const script of scripts) {
        results.push(await loadIconSet(script));
    }
    return results;
}

export async function loadModel(element: Element): Promise<dataModel.DataModel> {
    const text = await loadScript(element);
    return dataModel.Convert.toDataModel(text);
}

