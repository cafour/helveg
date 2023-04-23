import { createInstance, initializeInstance, type HelvegExtensions, type HelvegInstance } from "./instance";

declare global {
    interface Window {
        helveg: HelvegInstance & HelvegExtensions;
    }

    var helveg: HelvegInstance & HelvegExtensions;
}

export function initializeGlobal() {
    window.helveg = createInstance();
    initializeInstance(window.helveg)
        .catch(console.error);
}
