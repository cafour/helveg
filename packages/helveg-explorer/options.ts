import { DEFAULT_CUT_OPTIONS, type CutOptions, type ExportOptions, DEFAULT_EXPORT_OPTIONS, PizzaIcons, DEFAULT_CSHARP_RELATION_COLORS, type ForceAtlas2Options, DEFAULT_FORCE_ATLAS2_OPTIONS, type SizingMode } from "./deps/helveg-diagram.ts";
export { type CutOptions, type ExportOptions, DEFAULT_EXPORT_OPTIONS };

export interface ShowPropertiesToolOptions {
    shouldHighlightSubtree: boolean;
    shouldHighlightNeighbors: boolean;
}

export const DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS: Readonly<ShowPropertiesToolOptions> = {
    shouldHighlightSubtree: false,
    shouldHighlightNeighbors: false
};

export interface ToolOptions {
    cut: CutOptions;
    showProperties: ShowPropertiesToolOptions;
}

export const DEFAULT_TOOL_OPTIONS: Readonly<ToolOptions> = {
    cut: DEFAULT_CUT_OPTIONS,
    showProperties: DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS
}

export interface GlyphOptions {
    showIcons: boolean;
    showOutlines: boolean;
    showLabels: boolean;
    showFire: boolean;
    showDiffs: boolean;
    isFireAnimated: boolean;
    dimCollapsedNodes: boolean;
    codePizza: boolean;
    sizingMode: SizingMode
}

export const DEFAULT_GLYPH_OPTIONS: GlyphOptions = {
    showIcons: true,
    showOutlines: true,
    showLabels: true,
    showDiffs: true,
    showFire: true,
    isFireAnimated: true,
    dimCollapsedNodes: true,
    codePizza: false,
    sizingMode: "linear"
}

export interface CodePizzaOptions {
    isEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
    pizzaToppings?: Record<string, PizzaIcons>;
}

export const DEFAULT_CODE_PIZZA_OPTIONS: CodePizzaOptions = {
    isEnabled: false,
    crustWidth: 20,
    sauceWidth: 40
};

export interface AppearanceOptions {
    glyph: GlyphOptions;
    codePizza: CodePizzaOptions;
    relationColors?: Record<string, string>;
}

export const DEFAULT_APPEARANCE_OPTIONS: Readonly<AppearanceOptions> = {
    glyph: DEFAULT_GLYPH_OPTIONS,
    codePizza: DEFAULT_CODE_PIZZA_OPTIONS,
    relationColors: DEFAULT_CSHARP_RELATION_COLORS
};

export interface TidyTreeOptions {
    relation: string | null;
}

export const DEFAULT_TIDY_TREE_OPTIONS: Readonly<TidyTreeOptions> = {
    relation: null
}

export interface LayoutOptions {
    forceAtlas2: ForceAtlas2Options;
    tidyTree: TidyTreeOptions;
}

export const DEFAULT_LAYOUT_OPTIONS: Readonly<LayoutOptions> = {
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS,
    tidyTree: DEFAULT_TIDY_TREE_OPTIONS
};

export interface DataOptions {
    selectedRelations: string[];
    selectedKinds: string[];
    expandedDepth?: number;
}

export const DEFAULT_DATA_OPTIONS: Readonly<DataOptions> = {
    selectedRelations: [],
    selectedKinds: [],
}

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
    tool: DEFAULT_TOOL_OPTIONS
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
