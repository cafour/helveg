import { StaticGlyphStyle, type GlyphStyle, Outline, LineStyle } from "./glyph";

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
        outlines: [new Outline(LineStyle.Solid, "#202020", 1)]
    });
    showIcons: boolean = true;
    showOutlines: boolean = true;
}

export class LayoutOptions {
    
}

export interface ForceAtlas2Options {
    gravity: number;
    
}
