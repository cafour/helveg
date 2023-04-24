import type { GlyphStyle, GlyphStyleRegistry } from "./glyph";
import type { IconRegistry, IconSet } from "./icons";
import type { HelvegInstance } from "./instance";

export interface HelvegPluginContext {
    iconSets: Record<string, IconSet>;
    styles: Record<string, GlyphStyle>;
}

export interface HelvegPlugin {
    name: string;
    setup(context: HelvegPluginContext): void;
}

export class HelvegPluginRegistry {
    private data: Record<string, HelvegPlugin> = {};
    
    constructor(private iconRegistry: IconRegistry, private styleRegistry: GlyphStyleRegistry) {
    }

    register(plugin: HelvegPlugin) {
        if (this.data[plugin.name]) {
            throw new Error(`A plugin with the name '${plugin.name}' has already been registered.`);
        }

        this.data[plugin.name] = plugin;
        
        let context: HelvegPluginContext = {
            iconSets: {},
            styles: {}
        };
        plugin.setup(context);
        
        for (let [_, iconSet] of Object.entries(context.iconSets)) {
            this.iconRegistry.register(iconSet);
        }
        
        for (let [_, style] of Object.entries(context.styles)) {
            this.styleRegistry.register(style);
        }
    }

    get(name: string): HelvegPlugin | null {
        return this.data[name] ?? null;
    }
}
