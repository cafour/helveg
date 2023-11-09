import { csharpEdgeStylist, csharpNodeStylist } from "./csharp/style.ts";
import { IconRegistry, IconSet } from "./model/icons.ts";
import { DEFAULT_ICON_ATLAS_OPTIONS } from "./rendering/iconAtlas.ts"
import { LogSeverity, consoleLogger } from "./model/logger.ts";
import { EdgeStylist, NodeStylist } from "./model/style.ts";
import { EMPTY_MODEL, VisualizationModel } from "./model/visualization.ts";
import { IconAtlas } from "./rendering/iconAtlas.ts";
import { DEFAULT_GLYPH_PROGRAM_OPTIONS } from "./rendering/node.glyph.ts";
import { loadJsonScripts, requireJsonScript } from "./model/data.ts";
import { Diagram } from "./diagram/diagram.ts";

export type { Diagram, VisualizationModel, IconSet, IconAtlas };

export interface CreateDiagramOptions {
    element: HTMLElement | null,
    iconSets: IconSet[],
    model: VisualizationModel,
    logLevel: LogSeverity,
    nodeStylist: NodeStylist,
    edgeStylist: EdgeStylist,
    mainRelation: string | null,
    iconSize: number
}

export const DEFAULT_CREATE_DIAGRAM_OPTIONS: CreateDiagramOptions = {
    logLevel: LogSeverity.Info,
    model: EMPTY_MODEL,
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
        glyphProgram: {
            ...DEFAULT_GLYPH_PROGRAM_OPTIONS,
            iconAtlas: new IconAtlas(iconRegistry, { iconSize: opts.iconSize })
        }
    })
    diagram.model = opts.model;
    return diagram;
}

export function loadIconSet(element: HTMLElement): Promise<IconSet> {
    return requireJsonScript(element);
}

export function loadIconSets(selector: string): Promise<IconSet[]> {
    return loadJsonScripts(selector);
}

export function loadModel(element: HTMLElement): Promise<VisualizationModel> {
    return requireJsonScript(element);
}
