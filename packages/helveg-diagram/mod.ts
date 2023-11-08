import * as helvegGlobal from "./global.ts";

declare global {
    interface Window {
        helveg: typeof helvegGlobal
    }

    const helveg: typeof helvegGlobal;
}

window.helveg = helvegGlobal;
