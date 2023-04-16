import type { GraphNode } from "./multigraph";
import { DataOptions, GlyphOptions, LayoutOptions } from "./options";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

export enum StructuralStatus {
    Stopped,
    Running,
    RunningInBackground
}

export class StructuralState {
    selectedNode: GraphNode | null = null;
    dataOptions: DataOptions = new DataOptions();
    glyphOptions: GlyphOptions = new GlyphOptions();
    layoutOptions: LayoutOptions = new LayoutOptions();
    status: StructuralStatus = StructuralStatus.Stopped;

    applyPlugin(plugin: VisualizationPlugin) {
        let context: VisualizationPluginContext = {
            dataOptions: this.dataOptions,
            glyphOptions: this.glyphOptions
        };

        plugin.setup(context);
    }
}
