import { HelvegEvent } from "./common/event.ts";
import * as helvegGlobal from "./global.ts";
export * from "./global.ts";

type HelvegGlobal = typeof helvegGlobal;

declare global {

    interface Helveg extends HelvegGlobal {
    }

    interface Window {
        helveg: Helveg
    }

    const helveg: Helveg;
}

const globalThisExtended = globalThis as typeof globalThis & { helveg: Helveg }

globalThisExtended.helveg = globalThisExtended.helveg ?? {};
Object.assign(globalThisExtended.helveg, helvegGlobal);
