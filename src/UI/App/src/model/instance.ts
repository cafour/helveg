import { HelvegEvent } from "common/event";
import { loadJsonScripts } from "./data";
import type { IconSet } from "./icons";
import { EMPTY_MODEL, type VisualizationModel } from "./visualization";
import App from "components/App.svelte";
import { HelvegPluginRegistry } from "./plugin";
import { IconRegistry } from "./icons";
import { DEFAULT_HELVEG_OPTIONS, loadOptions, type HelvegOptions, type ExportOptions, type LayoutOptions, type ToolOptions, clearOptions, saveOptions } from "./options";
import { LogSeverity, Logger } from "./logger";
import { NodeStyleRegistry, EdgeStyleRegistry } from "./style";
import { UIExtensionRegistry } from "./uiExtensions";
import type { DataOptions, GlyphOptions } from "helveg";

export interface HelvegInstance {
    model: VisualizationModel;
    loaded: HelvegEvent<VisualizationModel>;
    optionsChanged: HelvegEvent<HelvegOptions>;
    stateImported: HelvegEvent<HelvegSerializedState>;
    nodeStyles: NodeStyleRegistry;
    edgeStyles: EdgeStyleRegistry;
    icons: IconRegistry;
    uiExtensions: UIExtensionRegistry;
    plugins: HelvegPluginRegistry;
    app: App | null;
    options: HelvegOptions;
    logger: Logger;

    resetOptions(): void;
    importState(state: HelvegSerializedState): void;
    exportState(): HelvegSerializedState;
};

export interface HelvegSerializedState {
    options: HelvegOptions;
    positions: Record<string, { x: number, y: number }>;
}

export function createHelvegInstance(): HelvegInstance {
    let iconRegistry = new IconRegistry();
    let nodeStyleRegistry = new NodeStyleRegistry();
    let edgeStyleRegistry = new EdgeStyleRegistry();
    let uiExtensionRegistry = new UIExtensionRegistry();
    let pluginRegistry = new HelvegPluginRegistry(
        iconRegistry,
        nodeStyleRegistry,
        edgeStyleRegistry,
        uiExtensionRegistry);
    let options: HelvegOptions = { ...DEFAULT_HELVEG_OPTIONS };
    options.data = loadOptions<DataOptions>("data") ?? options.data;
    options.export = loadOptions<ExportOptions>("export") ?? options.export;
    options.glyph = loadOptions<GlyphOptions>("glyph") ?? options.glyph;
    options.layout = loadOptions<LayoutOptions>("layout") ?? options.layout;
    options.tool = loadOptions<ToolOptions>("tool") ?? options.tool;

    return {
        model: EMPTY_MODEL,
        loaded: new HelvegEvent<VisualizationModel>("helveg.loaded", true),
        optionsChanged: new HelvegEvent<HelvegOptions>("helveg.optionsChanged", true),
        stateImported: new HelvegEvent<HelvegSerializedState>("helveg.stateImported", true),
        nodeStyles: nodeStyleRegistry,
        edgeStyles: edgeStyleRegistry,
        icons: iconRegistry,
        uiExtensions: uiExtensionRegistry,
        plugins: pluginRegistry,
        app: null,
        options: options,
        logger: new Logger(),

        resetOptions() {
            Object.assign(this.options, { ...DEFAULT_HELVEG_OPTIONS });
            clearOptions("data");
            clearOptions("export");
            clearOptions("glyph");
            clearOptions("layout");
            clearOptions("tool");
            this.optionsChanged.trigger(this.options);
            this.logger.info("Options reset.");
        },

        importState(state: HelvegSerializedState): void {
            this.options = state.options;
            saveOptions("data", state.options.data);
            saveOptions("export", state.options.export);
            saveOptions("glyph", state.options.glyph);
            saveOptions("layout", state.options.layout);
            saveOptions("tool", state.options.tool);
            this.optionsChanged.trigger(this.options);

            if (!this.app) {
                console.warn("Cannot import positions: App not initialized.");
            } else {
                this.app.getDiagram()?.importPositions(state.positions);
            }

            this.logger.info("State imported.");

            this.stateImported.trigger(state);
        },

        exportState(): HelvegSerializedState {
            let result: HelvegSerializedState = {
                options: this.options,
                positions: {}
            };

            if (!this.app) {
                console.warn("Cannot export positions: App not initialized.")
                return result;
            }

            result.positions = this.app.getDiagram()?.exportPositions() ?? {};
            return result;
        }
    };
}

export async function initializeHelvegInstance(instance: HelvegInstance): Promise<void> {
    instance.app = new App({
        target: document.getElementById("app")!,
        props: {
            instance: instance
        },
        hydrate: false
    });

    let iconSets = await loadIcons();
    for (let namespace in iconSets) {
        instance.icons.register(iconSets[namespace]);
    }

    let dataCandidates = await loadJsonScripts<VisualizationModel>("script#helveg-data");
    let model: VisualizationModel | null = null;
    if (dataCandidates.length === 0) {
        console.warn("No data found. Using empty visualization model.");
        model = EMPTY_MODEL;
    }
    else {
        model = dataCandidates[0];
        DEBUG && console.log(`Model '${model.documentInfo.name}' loaded.`);
    }

    instance.model = model;
    instance.loaded.trigger(model);
}

export async function loadIcons(): Promise<Record<string, IconSet>> {
    let result: Record<string, IconSet> = {};
    let iconSets = await loadJsonScripts<IconSet>("script.helveg-icons");
    iconSets.forEach(s => result[s.namespace] = s);
    return result;
}
