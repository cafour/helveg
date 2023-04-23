import { HelvegEvent } from "common/event";
import { loadJsonScripts } from "./data";
import { loadIcons, type IconSet } from "./icons";
import { EMPTY_MODEL as emptyModel, type VisualizationModel } from "./visualization";
import { GlyphStyleRepository } from "./glyph";
import App from "components/App.svelte";
import { HelvegPluginRegistry } from "./plugin";

export interface HelvegInstance {
    glyphStyleRepository: GlyphStyleRepository;
    iconSets: Record<string, IconSet>;
    model: VisualizationModel;
    modelLoaded: HelvegEvent<void>;
    iconsLoaded: HelvegEvent<void>;
    app: App | null;
    plugins: HelvegPluginRegistry;
};

/**
 * An interface to be used to extend the `helveg` global object;
 */
export interface HelvegExtensions {
}

export function createInstance(): HelvegInstance {
    return <HelvegInstance>{
        glyphStyleRepository: new GlyphStyleRepository(),
        iconSets: {},
        model: emptyModel,
        modelLoaded: new HelvegEvent<void>("helveg.modelLoaded", true),
        iconsLoaded: new HelvegEvent<void>("helveg.iconsLoaded", true),
        app: null,
        plugins: new HelvegPluginRegistry()
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
    let dataCandidates = await loadJsonScripts<VisualizationModel>("script#helveg-data");
    let model: VisualizationModel | null = null;
    if (dataCandidates.length === 0) {
        console.warn("No data found. Using empty visualization model.");
        model = emptyModel;
    }
    else {
        model = dataCandidates[0];
        DEBUG && console.log("Model loaded.");
    }

    instance.iconSets = iconSets;
    instance.iconsLoaded.trigger();
    instance.model = model;
    instance.modelLoaded.trigger();
}
