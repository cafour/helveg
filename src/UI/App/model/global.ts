import { loadJsonScripts } from "./data";
import { loadIcons, type IconSet } from "./icons";
import { empty as emptyModel, type VisualizationModel } from "./visualization";

export interface HelvegGlobal {
    iconSets: Record<string, IconSet>;
    model: VisualizationModel;
    isLoaded: boolean;
    loadingPromise: Promise<void>;
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
    window.helveg.model = model;
    window.helveg.isLoaded = true;
}

window.helveg = {
    iconSets: {},
    model: emptyModel,
    isLoaded: false,
    loadingPromise: initialize()
};
window.helveg.loadingPromise.catch(e => console.error(e));

export default window.helveg;
