import type { DataOptions, GlyphOptions } from "./options";

export interface VisualizationPluginContext
{
    dataOptions: DataOptions;
    glyphOptions: GlyphOptions;
}

export interface VisualizationPlugin
{
    name: string;
    setup(context: VisualizationPluginContext): void;
}
