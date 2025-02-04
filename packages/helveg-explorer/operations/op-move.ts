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
