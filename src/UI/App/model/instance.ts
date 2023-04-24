import { HelvegEvent } from "common/event";
import { loadJsonScripts } from "./data";
import type { IconSet } from "./icons";
import { EMPTY_MODEL, type VisualizationModel } from "./visualization";
import { GlyphStyleRegistry } from "./glyph";
import App from "components/App.svelte";
import { HelvegPluginRegistry } from "./plugin";
import { IconRegistry } from "./icons";
import { DEFAULT_HELVEG_OPTIONS, type HelvegOptions } from "./options";

export interface HelvegInstance {
    model: VisualizationModel;
    loaded: HelvegEvent<VisualizationModel>;
    styles: GlyphStyleRegistry;
    icons: IconRegistry;
    app: App | null;
    plugins: HelvegPluginRegistry;
    options: HelvegOptions;
};

export function createInstance(): HelvegInstance {
    let iconRegistry = new IconRegistry();
    let styleRegistry = new GlyphStyleRegistry();
    let pluginRegistry = new HelvegPluginRegistry(iconRegistry, styleRegistry);

    return <HelvegInstance>{
        model: EMPTY_MODEL,
        loaded: new HelvegEvent<VisualizationModel>("helveg.loaded", true),
        styles: styleRegistry,
        icons: iconRegistry,
        plugins: pluginRegistry,
        app: null,
        options: DEFAULT_HELVEG_OPTIONS
    };
}

export async function initializeInstance(instance: HelvegInstance): Promise<void> {
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
        DEBUG && console.log(`Model '${model.multigraph.label}' loaded.`);
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
