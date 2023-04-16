import type { GraphNode } from "./multigraph";
import { DataOptions, GlyphOptions } from "./options";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

export class StructuralState {
    selectedNode: GraphNode | null = null;
    dataOptions: DataOptions = new DataOptions();
    glyphOptions: GlyphOptions = new GlyphOptions();

    applyPlugin(plugin: VisualizationPlugin) {
        let context: VisualizationPluginContext = {
            dataOptions: this.dataOptions,
            glyphOptions: this.glyphOptions
        };

        plugin.setup(context);
    }
}
