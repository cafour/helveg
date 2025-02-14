import { get } from "svelte/store";
import { AppIcons } from "../const";
import { ModifierFlags, MouseButton, OperationScope, type NodeOperation, type Operation } from "./executor";

export const OP_MOVE: NodeOperation = {
    id: "move",
    name: "Move",
    hint: "Manually move a node by dragging and dropping.",
    icon: AppIcons.MoveTool,
    scopes: OperationScope.NODE,
    shortcut: {
        key: "m",
    },
    gesture: {
        button: MouseButton.MAIN,
        modifiers: ModifierFlags.SHIFT,
    },

    async beginExecute(state, nodeId) {
        state.diagram.dragNode(nodeId);

        if (get(state.toolOptions).move.shouldRunLayout) {
            await state.diagram.runLayout(false);
        }
    },

    endExecute(state, _nodeId) {
        state.diagram.dragNode(null);
    },
};

export const OP_MOVE_STOP: NodeOperation = {
    id: "move-stop",
    name: "Stop move",
    hint: "Stop moving when the user releases shift.",
    scopes: OperationScope.GLOBAL | OperationScope.NODE,
    shortcut: {
        key: "Shift",
    },

    keyUp(state) {
        state.diagram.dragNode(null);
    },
};
