import { AppIcons } from "../const";
import { MouseButton, OperationScope, type GlobalOperation, type NodeOperation } from "./executor";

export const OP_INSPECT: NodeOperation = {
    id: "inspect",
    name: "Inspect",
    hint: "Show the properties of a node.",
    scope: OperationScope.NODE,
    icon: AppIcons.ShowPropertiesTool,
    shortcut: {
        key: "i",
    },
    gesture: {
        button: MouseButton.MAIN,
    },

    beginExecute(state, nodeId) {
        state.diagram.selectedNode = nodeId;
    },
};

export const OP_DESELECT: GlobalOperation = {
    id: "deselect",
    name: "Deselect",
    scope: OperationScope.GLOBAL,
    shortcut: {
        key: "Escape",
    },
    gesture: {
        button: MouseButton.MAIN,
    },

    keyDown(state) {
        state.diagram.selectedNode = null;
    },

    mouseDown(state, _contex, event) {
        if (!event.hasMoved) {
            state.diagram.selectedNode = null;
        }
    },
};
