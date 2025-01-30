import { AppIcons } from "../const";
import { ModifierFlags, MouseButton, OperationScope, type NodeOperation } from "./executor";

export const OP_REMOVE: NodeOperation = {
    id: "remove",
    name: "Remove",
    hint: "Removes nodes and (optionally) its descendants.",
    shortcut: {
        key: "Delete",
    },
    gesture: {
        button: MouseButton.MAIN,
        modifiers: ModifierFlags.ALT,
    },
    icon: AppIcons.RemoveTool,
    scope: OperationScope.NODE,

    async beginExecute(state, nodeId) {
        await state.diagram.remove(nodeId);
    }
};
