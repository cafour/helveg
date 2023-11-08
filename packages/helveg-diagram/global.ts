import { csharpEdgeStylist, csharpNodeStylist } from "./csharp/style.ts";
import { Diagram } from "./diagram/diagram.ts";
import { IconRegistry, IconSet } from "./model/icons.ts";
import { DEFAULT_ICON_ATLAS_OPTIONS } from "./rendering/iconAtlas.ts"
import { LogSeverity } from "./model/logger.ts";
import { EdgeStylist, NodeStylist } from "./model/style.ts";
import { EMPTY_MODEL, VisualizationModel } from "./model/visualization.ts";
import { IconAtlas } from "./rendering/iconAtlas.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS } from "./rendering/node.glyph.ts";
import { loadJsonScript, loadJsonScripts } from "./model/data.ts";

export interface CreateDiagramOptions {
    element: HTMLElement | null,
    iconSets: IconSet[],
    model: VisualizationModel,
    logLevel: LogSeverity,
    nodeStylist: NodeStylist,
    edgeStylist: EdgeStylist,
    mainRelation: string,
    iconSize: number
}

export const DEFAULT_CREATE_DIAGRAM_OPTIONS: CreateDiagramOptions = {
    logLevel: LogSeverity.Info,
    model: EMPTY_MODEL,
    iconSets: [],
    element: null,
    nodeStylist: csharpNodeStylist,
    edgeStylist: csharpEdgeStylist,
    mainRelation: "contains",
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

    const iconRegistry = new IconRegistry();
    opts.iconSets?.forEach(s => iconRegistry.register(s));

    return new Diagram(opts.element, {
        logLevel: opts.logLevel,
        nodeStylist: opts.nodeStylist,
        edgeStylist: opts.edgeStylist,
        mainRelation: opts.mainRelation,
        glyphProgram: {
            ...DEFAULT_GLYPH_PROGRAM_OPTIONS,
            iconAtlas: new IconAtlas(iconRegistry, { iconSize: opts.iconSize })
        }
    })
}

export async function loadIconSet(element: HTMLElement): Promise<IconSet> {
    const result = await loadJsonScript<IconSet>(element);
    if (!result) {
        throw new Error(`No iconSet could be read from element ${element}.`);
    }

    return result;
}

export function loadIconSets(selector: string): Promise<IconSet[]> {
    return loadJsonScripts(selector);
}
