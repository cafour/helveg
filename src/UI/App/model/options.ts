import { StaticGlyphStyle, type GlyphStyle, type Outline, OutlineStyle, FALLBACK_ICON_NAME as DEFAULT_GLYPH_ICON_NAME } from "./glyph";

export interface DataOptions {
    kinds: string[];
    selectedKinds: string[];
    defaultIcons: Record<string, string>;
    fallbackIcon: string;
    selectedRelations: string[];
}

export const DEFAULT_DATA_OPTIONS: DataOptions = {
    kinds: [],
    selectedKinds: [],
    defaultIcons: {},
    fallbackIcon: DEFAULT_GLYPH_ICON_NAME,
    selectedRelations: ["declares"]
}

export interface GlyphOptions {
    styles: Record<string, GlyphStyle>;
    fallbackStyle: GlyphStyle;
    showIcons: boolean;
    showOutlines: boolean;
    showLabels: boolean;
}

export const DEFAULT_GLYPH_OPTIONS: GlyphOptions = {
    styles: {},
    fallbackStyle: new StaticGlyphStyle({
        size: 5,
        color: "#202020",
        icon: DEFAULT_GLYPH_ICON_NAME,
        outlines: []
    }),
    showIcons: true,
    showOutlines: true,
    showLabels: true
}

export interface ForceAtlas2Options {
    gravity: number;
}

export const DEFAULT_FORCE_ATLAS2_OPTIONS: ForceAtlas2Options = {
    gravity: 1
};

export interface LayoutOptions {
    hierarchyRelation: string;
    forceAtlas2: ForceAtlas2Options;
}

export const DEFAULT_LAYOUT_OPTIONS: LayoutOptions = {
    hierarchyRelation: "declares",
    forceAtlas2: DEFAULT_FORCE_ATLAS2_OPTIONS
};

export interface ExportOptions {
    fileName: string | null;
    includeEdges: boolean;
    includeNodes: boolean;
    includeLabels: boolean;
    backgroundColor: string;
    scale: number;
}

export const DEFAULT_EXPORT_OPTIONS: ExportOptions = {
    fileName: null, // let the diagram decide the name
    includeEdges: true,
    includeNodes: true,
    includeLabels: true,
    backgroundColor: "#ffffff",
    scale: 1
}

export interface HelvegOptions {
    layout: LayoutOptions;
    data: DataOptions;
    glyph: GlyphOptions;
    export: ExportOptions;
}

export const DEFAULT_HELVEG_OPTIONS: HelvegOptions = {
    layout: DEFAULT_LAYOUT_OPTIONS,
    data: DEFAULT_DATA_OPTIONS,
    glyph: DEFAULT_GLYPH_OPTIONS,
    export: DEFAULT_EXPORT_OPTIONS
};
