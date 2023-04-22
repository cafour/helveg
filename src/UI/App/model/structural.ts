import type { GraphNode } from "./multigraph";
import { DEFAULT_DATA_OPTIONS, DEFAULT_EXPORT_OPTIONS, DEFAULT_GLYPH_OPTIONS, DEFAULT_LAYOUT_OPTIONS, type DataOptions, type ExportOptions, type GlyphOptions, type LayoutOptions } from "./options";
import type { VisualizationPlugin, VisualizationPluginContext } from "./plugin";

export enum StructuralStatus {
    Stopped,
    Running,
    RunningInBackground
}

export class StructuralState {
    selectedNode: GraphNode | null = null;
    dataOptions: DataOptions = { ...DEFAULT_DATA_OPTIONS };
    glyphOptions: GlyphOptions = { ...DEFAULT_GLYPH_OPTIONS };
    layoutOptions: LayoutOptions = { ...DEFAULT_LAYOUT_OPTIONS };
    exportOptions: ExportOptions = { ...DEFAULT_EXPORT_OPTIONS };
    status: StructuralStatus = StructuralStatus.Stopped;

    applyPlugin(plugin: VisualizationPlugin) {
        let context: VisualizationPluginContext = {
            dataOptions: this.dataOptions,
            glyphOptions: this.glyphOptions
        };

        plugin.setup(context);
    }
}
