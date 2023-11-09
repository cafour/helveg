import type { CutOptions, ExportOptions } from "./deps/helveg-diagram.ts";
export { type CutOptions, type ExportOptions } from "./deps/helveg-diagram.ts";

export interface ToggleToolOptions {
    relation: string | null;
}

export const DEFAULT_TOGGLE_TOOL_OPTIONS: ToggleToolOptions = {
    relation: null
};

export interface ShowPropertiesToolOptions {
    shouldHighlightSubtree: boolean;
    shouldHighlightNeighbors: boolean;
}

export const DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS: ShowPropertiesToolOptions = {
    shouldHighlightSubtree: false,
    shouldHighlightNeighbors: false
};

export interface ToolOptions {
    cut: CutOptions;
    toggle: ToggleToolOptions;
    showProperties: ShowPropertiesToolOptions;
}

export const DEFAULT_TOOL_OPTIONS: ToolOptions = {
    cut: helveg.DEFAULT_CUT_OPTIONS,
    toggle: DEFAULT_TOGGLE_TOOL_OPTIONS,
    showProperties: DEFAULT_SHOW_PROPERTIES_TOOL_OPTIONS
}

export interface GlyphOptions {
    showIcons: boolean;
    showOutlines: boolean;
    showLabels: boolean;
    showFire: boolean;
    isFireAnimated: boolean;
    codePizza: boolean;
}

export const DEFAULT_GLYPH_OPTIONS: GlyphOptions = {
    showIcons: true,
    showOutlines: true,
    showLabels: true,
    showFire: true,
    isFireAnimated: true,
    codePizza: false
}

export interface CodePizzaOptions {
    isEnabled: boolean;
    crustWidth: number;
    sauceWidth: number;
}

export const DEFAULT_CODE_PIZZA_OPTIONS: CodePizzaOptions = {
    isEnabled: false,
    crustWidth: 20,
    sauceWidth: 40
};

export interface AppearanceOptions {
    glyph: GlyphOptions;
    codePizza: CodePizzaOptions;
}

export const DEFAULT_APPEARANCE_OPTIONS: AppearanceOptions = {
    glyph: DEFAULT_GLYPH_OPTIONS,
    codePizza: DEFAULT_CODE_PIZZA_OPTIONS
};

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

export interface DataOptions {
    selectedRelations: string[];
}

export const DEFAULT_DATA_OPTIONS: DataOptions = {
    selectedRelations: []
}

export interface ExplorerOptions {
    layout: LayoutOptions;
    data: DataOptions;
    appearance: AppearanceOptions;
    export: ExportOptions;
    tool: ToolOptions;
}

export const DEFAULT_HELVEG_OPTIONS: ExplorerOptions = {
    layout: DEFAULT_LAYOUT_OPTIONS,
    data: DEFAULT_DATA_OPTIONS,
    appearance: DEFAULT_APPEARANCE_OPTIONS,
    export: helveg.DEFAULT_EXPORT_OPTIONS,
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
