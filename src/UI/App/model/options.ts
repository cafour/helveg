import { Colors } from "./const";
import { FireStatus, type NodeStyle } from "./style";

export enum SearchMode {
    Contains = "contains",
    Regex = "regex",
    JavaScript = "js"
}

export interface DataOptions {
    selectedRelations: string[];
}

export const DEFAULT_DATA_OPTIONS: DataOptions = {
    selectedRelations: [],
    csharp: null!
}

export interface ToolOptions {
    isCuttingTransitive: boolean;
    cuttingRelation: string | null;
    collapsingRelation: string | null;
}

export const DEFAULT_TOOL_OPTIONS: ToolOptions = {
    isCuttingTransitive: true,
    cuttingRelation: null,
    collapsingRelation: null
}

export interface GlyphOptions {
    showIcons: boolean;
    showOutlines: boolean;
    showLabels: boolean;
    showFire: boolean;
    isFireAnimated: boolean;
}

export const DEFAULT_GLYPH_OPTIONS: GlyphOptions = {
    showIcons: true,
    showOutlines: true,
    showLabels: true,
    showFire: true,
    isFireAnimated: true
}

export interface ForceAtlas2Options {
    gravity: number;
}

export const DEFAULT_FORCE_ATLAS2_OPTIONS: ForceAtlas2Options = {
    gravity: 1
};

export interface TidyTreeOptions {
    relation: string | null;
}

export const DEFAULT_TIDY_TREE_OPTIONS: TidyTreeOptions = {
    relation: null
}

export interface LayoutOptions {
    forceAtlas2: ForceAtlas2Options;
    tidyTree: TidyTreeOptions;
}

export const DEFAULT_LAYOUT_OPTIONS: LayoutOptions = {
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS,
    tidyTree: DEFAULT_TIDY_TREE_OPTIONS
};

export interface ExportOptions {
    fileName: string | null;
    includeEdges: boolean;
    includeNodes: boolean;
    includeLabels: boolean;
    includeEffects: boolean;
    includeHighlights: boolean;
    backgroundColor: string;
    scale: number;
}

export const DEFAULT_EXPORT_OPTIONS: ExportOptions = {
    fileName: null, // let the diagram decide the name
    includeEdges: true,
    includeNodes: true,
    includeLabels: true,
    includeEffects: true,
    includeHighlights: true,
    backgroundColor: Colors.White,
    scale: 1
}

export interface HelvegOptions {
    layout: LayoutOptions;
    data: DataOptions;
    glyph: GlyphOptions;
    export: ExportOptions;
    tool: ToolOptions;
}

export const DEFAULT_HELVEG_OPTIONS: HelvegOptions = {
    layout: DEFAULT_LAYOUT_OPTIONS,
    data: DEFAULT_DATA_OPTIONS,
    glyph: DEFAULT_GLYPH_OPTIONS,
    export: DEFAULT_EXPORT_OPTIONS,
    tool: DEFAULT_TOOL_OPTIONS
};
