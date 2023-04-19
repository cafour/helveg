import { HelvegEvent } from "common/event";
import { loadJsonScripts } from "./data";
import { loadIcons, type IconSet } from "./icons";
import { empty as emptyModel, type VisualizationModel } from "./visualization";

export interface HelvegGlobal {
    iconSets: Record<string, IconSet>;
    model: VisualizationModel;
    modelLoaded: HelvegEvent<void>;
    iconsLoaded: HelvegEvent<void>;
};

/**
 * An interface to be used to extend the `helveg` global object;
 */
export interface HelvegExtensions {
}

declare global {
    interface Window {
        helveg: HelvegGlobal & HelvegExtensions;
    }

    var helveg: HelvegGlobal & HelvegExtensions;
}

async function initialize() {
    let iconSets = await loadIcons();

    let dataCandidates = await loadJsonScripts<VisualizationModel>("script#helveg-data");
    let model: VisualizationModel | null = null;
    if (dataCandidates.length === 0) {
        console.warn("No data found. Using empty visualization model.");
        model = emptyModel;
    }
    else {
        model = dataCandidates[0];
    }

    window.helveg = window.helveg ?? {};
    window.helveg.iconSets = iconSets;
    window.helveg.iconsLoaded.trigger();
    window.helveg.model = model;
    window.helveg.modelLoaded.trigger();
}

window.helveg = {
    iconSets: {},
    model: emptyModel,
    modelLoaded: new HelvegEvent<void>("helveg.modelLoaded"),
    iconsLoaded: new HelvegEvent<void>("helveg.iconsLoaded"),
};

export default window.helveg;

initialize()
    .catch(e => console.error(e))
