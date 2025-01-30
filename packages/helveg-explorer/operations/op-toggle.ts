import { AppIcons } from "../const";
import { MouseButton, OperationScope, type NodeOperation } from "./executor";

export const OP_TOGGLE: NodeOperation = {
    id: "toggle",
    name: "Toggle",
    hint: "Collapses or expands the children of a node.",
    scopes: OperationScope.NODE,
    shortcut: {
        key: "t"
    },
    gesture: {
        button: MouseButton.MAIN,
        isDouble: true,
    },
    icon: AppIcons.ToggleTool,

    async beginExecute(state, nodeId) {
        await state.diagram.toggleNode(nodeId);
    }
}
