import { StaticGlyphStyle, type GlyphStyle, type Outline, OutlineStyle } from "./glyph";

export class DataOptions {
    kinds: string[] = [];
    selectedKinds: string[] = [];
    defaultIcons: Record<string, string> = {};
    fallbackIcon: string = "base:PolarChart";
}

export class GlyphOptions {
    styles: Record<string, GlyphStyle> = {};
    fallbackStyle: GlyphStyle = new StaticGlyphStyle({
        size: 5,
        color: "#202020",
        icon: "base:PolarChart",
        outlines: []
    });
    showIcons: boolean = true;
    showOutlines: boolean = true;
    showLabels: boolean = true;
}

export class LayoutOptions {
    
}

export interface ForceAtlas2Options {
    gravity: number;
    
}
