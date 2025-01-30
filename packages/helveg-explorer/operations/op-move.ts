import { AppIcons } from "../const";
import { ModifierFlags, MouseButton, OperationScope, type NodeOperation, type Operation } from "./executor";

export const OP_MOVE: NodeOperation = {
    id: "move",
    name: "Move",
    hint: "Manually move a node by draggin and dropping.",
    icon: AppIcons.MoveTool,
    scope: OperationScope.NODE,
    shortcut: {
        key: "m"
    },
    gesture: {
        button: MouseButton.MAIN,
        modifiers: ModifierFlags.SHIFT
    },

    beginExecute(state, nodeId) {
        state.diagram.dragNode(nodeId);
    },

    endExecute(state, _nodeId) {
        state.diagram.dragNode(null);
    }
};
