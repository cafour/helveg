import "styles/helveg.scss";

import type { HelvegOptions } from "model/options";
import { createInstance, initializeInstance, type HelvegInstance } from "model/instance";
import type { HelvegPlugin } from "model/plugin";
export * from "types";
import * as types from "types";

/**
 * An interface allowing anyone to extend the global helveg instance.
 */
export interface HelvegExtensions {
}

declare global {
    interface Window {
        helveg: HelvegInstance & HelvegExtensions & typeof types;
    }

    const helveg: HelvegInstance & HelvegExtensions & typeof types;
}

export function initializeGlobal(pluginFuncs?: ((options: HelvegOptions) => HelvegPlugin)[]) {
    window.helveg = {...types, ...createInstance()};
    pluginFuncs?.forEach(plugin => window.helveg.plugins.register(plugin(window.helveg.options)));

    initializeInstance(window.helveg)
        .catch(console.error);
}
