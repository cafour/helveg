import { createInstance, initializeInstance, type HelvegInstance } from "./instance";
import type { HelvegPlugin } from "./plugin";

/**
 * An interface allowing anyone to extend the global helveg instance.
 */
export interface HelvegExtensions {
}

declare global {
    interface Window {
        helveg: HelvegInstance & HelvegExtensions;
    }

    const helveg: HelvegInstance & HelvegExtensions;
}

export function initializeGlobal(plugins?: HelvegPlugin[]) {
    window.helveg = createInstance();
    plugins?.forEach(plugin => window.helveg.plugins.register(plugin));

    initializeInstance(window.helveg)
        .catch(console.error);
}
