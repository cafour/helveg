import type { DataOptions, GlyphOptions } from "./options";

export interface HelvegPluginContext
{
    dataOptions: DataOptions;
    glyphOptions: GlyphOptions;
}

export interface HelvegPlugin
{
    name: string;
    setup(context: HelvegPluginContext): void;
}


export class HelvegPluginRegistry
{
    private data: Record<string, HelvegPlugin> = {};

    public register(plugin: HelvegPlugin) {
        this.data[plugin.name] = plugin;
    }
    
    public get(name: string): HelvegPlugin | null {
        return this.data[name] ?? null;
    }
}
