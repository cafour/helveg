import type { GlyphStyle } from "./glyph";

export class DataOptions
{
    kinds: string[] = [];
    selectedKinds: string[] = [];
    defaultIcons: Record<string, string> = {};
}

export class GlyphOptions
{
    styles: Record<string, GlyphStyle> = {};
    showIcons: boolean = true;
    showOutlines: boolean = true;
}
