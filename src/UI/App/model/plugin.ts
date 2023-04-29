import type { GlyphStyle, GlyphStyleRegistry } from "./glyph";
import type { HelvegGraph } from "./graph";
import type { Icon, IconRegistry, IconSet } from "./icons";
import type { VisualizationModel } from "./visualization";

export interface HelvegPlugin {
    name: string;
    icons?: Map<string, Icon>;
    glyphStyles?: GlyphStyle[];
    onVisualize?(model: Readonly<VisualizationModel>, graph: HelvegGraph): void;
}

export class HelvegPluginRegistry {
    private names: Set<string> = new Set();
    private data: HelvegPlugin[] = [];

    constructor(private iconRegistry: IconRegistry, private styleRegistry: GlyphStyleRegistry) {
    }

    register(plugin: HelvegPlugin) {
        if (this.names.has(plugin.name)) {
            throw new Error(`A plugin with the name '${plugin.name}' has already been registered.`);
        }

        this.data.push(plugin);

        if (plugin.icons) {
            let iconSet = {
                namespace: plugin.name,
                icons: plugin.icons
            };
            this.iconRegistry.register(iconSet);
        }

        if (plugin.glyphStyles) {
            for (let style of plugin.glyphStyles) {
                this.styleRegistry.register(plugin.name, style);
            }
        }
    }

    get(name: string): HelvegPlugin | null {
        return this.data[name] ?? null;
    }

    getAll(): IterableIterator<HelvegPlugin> {
        return this.data.values();
    }
}
