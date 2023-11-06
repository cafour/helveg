import { HelvegInstance, initializeHelvegInstance } from "./model/instance.ts";

declare global {
    interface Window {
        helveg: HelvegInstance;
    }

    const helveg: HelvegInstance;
}

initializeHelvegInstance(window.helveg)
    .catch(console.error);
