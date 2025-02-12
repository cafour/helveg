import { DiagramStatus } from "@cafour/helveg-diagram";
import { ModifierFlags, OperationScope, type GlobalOperation } from "./executor";
import { get } from "svelte/store";

export const OP_LAYOUT: GlobalOperation = {
    id: "layout",
    name: "Layout",
    scopes: OperationScope.GLOBAL,
    hint: "Runs or stop a continuous layout algorithm.",
    icon: "vscode:play",
    shortcut: {
        key: " ",
    },
    async beginExecute(state) {
        if (state.diagram.status === DiagramStatus.Stopped) {
            await state.diagram.runLayout(false);
        } else {
            await state.diagram.stopLayout();
        }
    },
};

export const OP_REFRESH: GlobalOperation = {
    id: "refresh",
    name: "Refresh",
    scopes: OperationScope.GLOBAL,
    hint: "Rebuild the graph and re-run the automatic layout.",
    icon: "vscode:refresh",
    shortcut: {
        key: "l",
        modifiers: ModifierFlags.CONTROL,
    },
    async beginExecute(state) {
        const dataOptions = get(state.dataOptions);
        await state.diagram.refresh({
            selectedRelations: dataOptions.selectedRelations,
            selectedKinds: dataOptions.selectedKinds,
            expandedDepth: dataOptions.expandedDepth,
        });
    },
};

export const OP_AUTOLAYOUT: GlobalOperation = {
    id: "autolayout",
    name: "AutoLayout",
    scopes: OperationScope.GLOBAL,
    hint: "AutoLayout the current graph.",
    icon: "vscode:play-circle",
    shortcut: {
        key: "l",
    },
    async beginExecute(state) {
        await state.diagram.autoLayout();
        await state.diagram.runLayout(false);
    },
};
