import {
    type RemoveOptions,
    type ExportOptions,
    DEFAULT_EXPORT_OPTIONS,
    PizzaIcons,
    DEFAULT_CSHARP_RELATION_COLORS,
    type ForceAtlas2Options,
    DEFAULT_FORCE_ATLAS2_OPTIONS,
    type NodeColorSchema,
    SizingMode,
    UNIVERSAL_NODE_COLOR_SCHEMA,
    GlyphShape,
} from "./deps/helveg-diagram.ts";
export { type RemoveOptions, type ExportOptions, DEFAULT_EXPORT_OPTIONS };

export interface ShowPropertiesToolOptions {
    shouldHighlightSubtree: boolean;
    shouldHighlightNeighbors: boolean;
    shouldFixateSelectedNode: boolean;
    shouldFocusPropertiesPanel: boolean;
}

export const DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS: Readonly<ShowPropertiesToolOptions> = {
    shouldHighlightSubtree: false,
    shouldHighlightNeighbors: false,
    shouldFixateSelectedNode: true,
    shouldFocusPropertiesPanel: true,
};

export interface ToggleToolOptions {
    shouldRunLayout: boolean;
}

export const DEFAULT_TOGGLE_TOOL_OPTIONS: Readonly<ToggleToolOptions> = {
    shouldRunLayout: true,
};

export interface MoveToolOptions {
    shouldRunLayout: boolean;
}

export const DEFAULT_MOVE_TOOL_OPTIONS: Readonly<ToggleToolOptions> = {
    shouldRunLayout: false,
};

export interface RemoveToolOptions {
    isTransitive: boolean;
    shouldRunLayout: boolean;
}

export const DEFAULT_REMOVE_TOOL_OPTIONS: Readonly<RemoveToolOptions> = {
    isTransitive: true,
    shouldRunLayout: true,
};

export interface SearchToolOptions {
    shouldRunLayout: boolean;
}

export const DEFAULT_SEARCH_TOOL_OPTIONS: Readonly<SearchToolOptions> = {
    shouldRunLayout: true,
};

export interface ToolOptions {
    remove: RemoveToolOptions;
    showProperties: ShowPropertiesToolOptions;
    toggle: ToggleToolOptions;
    move: MoveToolOptions;
    search: SearchToolOptions;
}

export const DEFAULT_TOOL_OPTIONS: Readonly<ToolOptions> = {
    remove: DEFAULT_REMOVE_TOOL_OPTIONS,
    showProperties: DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS,
    toggle: DEFAULT_TOGGLE_TOOL_OPTIONS,
    move: DEFAULT_MOVE_TOOL_OPTIONS,
    search: DEFAULT_SEARCH_TOOL_OPTIONS,
};

export interface GlyphOptions {
    glyphShape: GlyphShape,
    showIcons: boolean;
    showLabels: boolean;
    showFire: boolean;
    showDiffs: boolean;
    showHatching: boolean;
    isFireAnimated: boolean;
    showCollapsedNodeIndicators: boolean;
    showContours: boolean;
    codePizza: boolean;
    sizingMode: SizingMode;
}

export const DEFAULT_GLYPH_OPTIONS: GlyphOptions = {
    glyphShape: GlyphShape.DONUT,
    showIcons: true,
    showLabels: true,
    showDiffs: true,
    showFire: true,
    showHatching: true,
    isFireAnimated: true,
    showCollapsedNodeIndicators: true,
    showContours: true,
    codePizza: false,
    sizingMode: SizingMode.LINEAR,
};

export interface CodePizzaOptions {
    isEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
    pizzaToppings?: Record<string, PizzaIcons>;
}

export const DEFAULT_CODE_PIZZA_OPTIONS: CodePizzaOptions = {
    isEnabled: false,
    crustWidth: 20,
    sauceWidth: 40,
};

export enum NodeColorSchemaPreset {
    Universal = "Universal",
    TypeFocus = "TypeFocus",
    VS = "VS",
    Custom = "Custom",
}

export interface AppearanceOptions {
    glyph: GlyphOptions;
    codePizza: CodePizzaOptions;
    relationColors?: Record<string, string>;
    nodeColorSchema: NodeColorSchema;
    nodeColorPreset: NodeColorSchemaPreset;
}

export const DEFAULT_APPEARANCE_OPTIONS: Readonly<AppearanceOptions> = {
    glyph: DEFAULT_GLYPH_OPTIONS,
    codePizza: DEFAULT_CODE_PIZZA_OPTIONS,
    relationColors: DEFAULT_CSHARP_RELATION_COLORS,
    nodeColorSchema: structuredClone(UNIVERSAL_NODE_COLOR_SCHEMA),
    nodeColorPreset: NodeColorSchemaPreset.Universal,
};

export interface TidyTreeOptions {
    relation: string | null;
}

export const DEFAULT_TIDY_TREE_OPTIONS: Readonly<TidyTreeOptions> = {
    relation: null,
};

export interface LayoutOptions {
    forceAtlas2: ForceAtlas2Options;
    tidyTree: TidyTreeOptions;
}

export const DEFAULT_LAYOUT_OPTIONS: Readonly<LayoutOptions> = {
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS,
    tidyTree: DEFAULT_TIDY_TREE_OPTIONS,
};

export interface DataOptions {
    selectedRelations: string[];
    selectedKinds: string[];
    expandedDepth?: number;
}

export const DEFAULT_DATA_OPTIONS: Readonly<DataOptions> = {
    selectedRelations: [],
    selectedKinds: [],
};

export interface ExplorerOptions {
    layout: LayoutOptions;
    data: DataOptions;
    appearance: AppearanceOptions;
    export: ExportOptions;
    tool: ToolOptions;
}

export const DEFAULT_HELVEG_OPTIONS: Readonly<ExplorerOptions> = {
    layout: DEFAULT_LAYOUT_OPTIONS,
    data: DEFAULT_DATA_OPTIONS,
    appearance: DEFAULT_APPEARANCE_OPTIONS,
    export: DEFAULT_EXPORT_OPTIONS,
    tool: DEFAULT_TOOL_OPTIONS,
};

export const STORAGE_KEY_PREFIX = "helveg.options.";

export function loadOptions<T>(name: string): T | null {
    const options = localStorage.getItem(STORAGE_KEY_PREFIX + name);
    if (options) {
        return JSON.parse(options);
    }
    return null;
}

export function saveOptions<T>(name: string, options: T) {
    localStorage.setItem(STORAGE_KEY_PREFIX + name, JSON.stringify(options));
}

export function clearOptions(name: string) {
    localStorage.removeItem(STORAGE_KEY_PREFIX + name);
}
