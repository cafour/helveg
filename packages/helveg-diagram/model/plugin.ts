import { HelvegGraph } from "./graph.ts";
import { Icon, IconRegistry } from "./icons.ts";
import { NodeStyleGenerator, EdgeStyleGenerator, NodeStyleRegistry, EdgeStyleRegistry } from "./style.ts";
import { VisualizationModel } from "./visualization.ts";

export interface HelvegPlugin {
    name: string;
    icons?: Map<string, Icon>;
    nodeStyles?: Map<string, NodeStyleGenerator>;
    edgeStyles?: Map<string, EdgeStyleGenerator>;
    // uiExtensions?: Map<string, UIExtension>;
    onVisualize?(model: Readonly<VisualizationModel>, graph: HelvegGraph): void;
}

export class HelvegPluginRegistry {
    private names: Set<string> = new Set();
    private data: HelvegPlugin[] = [];

    constructor(
        private iconRegistry: IconRegistry,
        private nodeStyles: NodeStyleRegistry,
        private edgeStyles: EdgeStyleRegistry) {
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

        if (plugin.nodeStyles) {
            for (let [key, value] of plugin.nodeStyles) {
                this.nodeStyles.register(`${plugin.name}:${key}`, value);
            }
        }
        
        if (plugin.edgeStyles) {
            for (let [key, value] of plugin.edgeStyles) {
                this.edgeStyles.register(`${plugin.name}:${key}`, value);
            }
        }
        
        if (plugin.uiExtensions) {
            for (let [key, value] of plugin.uiExtensions) {
                this.uiExtensions.register(`${plugin.name}:${key}`, value);
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
