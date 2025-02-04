import { get } from "svelte/store";
import { AppIcons } from "../const";
import { ModifierFlags, MouseButton, OperationScope, type GlobalOperation, type NodeOperation } from "./executor";

export const OP_TOGGLE: NodeOperation = {
    id: "toggle",
    name: "Toggle",
    hint: "Collapses or expands the children of a node.",
    scopes: OperationScope.NODE,
    shortcut: {
        key: "t",
    },
    gesture: {
        button: MouseButton.MAIN,
        isDouble: true,
    },
    icon: AppIcons.ToggleTool,

    async beginExecute(state, nodeId) {
        await state.diagram.toggleNode(nodeId);

        if (get(state.toolOptions).toggle.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },
};

export const OP_DIG_IN: GlobalOperation = {
    id: "dig-in",
    name: "Dig in",
    hint: "Expands all currently visible collapsed nodes -- increases the depth of the visualized graph by one.",
    scopes: OperationScope.GLOBAL | OperationScope.STAGE,
    shortcut: {
        key: "e",
    },
    icon: "vscode:diff-added",

    async beginExecute(state) {
        await state.diagram.dig(false);

        if (get(state.toolOptions).toggle.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },
};

export const OP_DIG_OUT: GlobalOperation = {
    id: "dig-out",
    name: "Dig out",
    hint: "Collapses the outermost visible nodes -- decreases the depth of the visualized graph by one.",
    scopes: OperationScope.GLOBAL | OperationScope.STAGE,
    shortcut: {
        key: "q",
    },
    icon: "vscode:diff-removed",

    async beginExecute(state) {
        await state.diagram.dig(true);

        if (get(state.toolOptions).toggle.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },
};

export const OP_EXPAND_ALL: GlobalOperation = {
    id: "expand-all",
    name: "Expand all",
    hint: "Expands all nodes of the graph.",
    scopes: OperationScope.GLOBAL | OperationScope.STAGE,
    shortcut: {
        key: "e",
        modifiers: ModifierFlags.CONTROL,
    },
    icon: "vscode:expand-all",

    async beginExecute(state) {
        await state.diagram.toggleAll(true);

        if (get(state.toolOptions).toggle.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },
};

export const OP_COLLAPSE_ALL: GlobalOperation = {
    id: "collapse-all",
    name: "Collapse all",
    hint: "Collapses all nodes of the graph.",
    scopes: OperationScope.GLOBAL | OperationScope.STAGE,
    shortcut: {
        key: "q",
        modifiers: ModifierFlags.CONTROL,
    },
    icon: "vscode:collapse-all",

    async beginExecute(state) {
        await state.diagram.toggleAll(false);

        if (get(state.toolOptions).toggle.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },
};
