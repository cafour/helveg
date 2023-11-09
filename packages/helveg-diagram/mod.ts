import * as helvegGlobal from "./global.ts";
export type * from "./global.ts";

type HelvegGlobal = typeof helvegGlobal;

declare global {

    interface Helveg extends HelvegGlobal
    {
    }

    interface Window {
        helveg: Helveg
    }

    const helveg: Helveg;
}

window.helveg = window.helveg ?? {};
Object.assign(window.helveg, helvegGlobal);
