import type { HelvegGraph } from "./graph";
import type { Icon, IconRegistry, IconSet } from "./icons";
import type { NodeStyleGenerator, NodeStyleRegistry, EdgeStyleGenerator, EdgeStyleRegistry } from "./style";
import type { VisualizationModel } from "./visualization";

export interface HelvegPlugin {
    name: string;
    icons?: Map<string, Icon>;
    nodeStyles?: Map<string, NodeStyleGenerator>;
    edgeStyles?: Map<string, EdgeStyleGenerator>;
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
    }

    get(name: string): HelvegPlugin | null {
        return this.data[name] ?? null;
    }

    getAll(): IterableIterator<HelvegPlugin> {
        return this.data.values();
    }
}
