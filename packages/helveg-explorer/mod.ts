import * as explorerGlobal from "./global.ts";

type ExplorerGlobal = typeof explorerGlobal;

declare global {
    interface Helveg extends ExplorerGlobal {
    }
}

window.helveg = window.helveg ?? {};
Object.assign(window.helveg, explorerGlobal);
