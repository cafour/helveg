import { AppIcons } from "../const";
import {
    MouseButton,
    OperationScope,
    type GlobalOperation,
    type NodeOperation,
    type StageOperation,
} from "./executor";

export const OP_INSPECT: NodeOperation = {
    id: "inspect",
    name: "Inspect",
    hint: "Show the properties of a node.",
    scopes: OperationScope.NODE,
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

export const OP_GLOBAL_DESELECT: GlobalOperation = {
    id: "global-deselect",
    name: "Deselect",
    hidden: true,
    scopes: OperationScope.GLOBAL,
    shortcut: {
        key: "Escape",
    },
    hint: "Deselect the currently selected node.",

    keyDown(state) {
        state.diagram.selectedNode = null;
        state.diagram.dragNode(null);
    },
};

export const OP_STAGE_DESELECT: StageOperation = {
    id: "stage-deselect",
    name: "Deselect",
    hidden: true,
    scopes: OperationScope.STAGE,
    gesture: {
        button: MouseButton.MAIN,
    },

    mouseUp(state, _context, event) {
        if (event.duration < 300 && event.distance < 100) {
            state.diagram.selectedNode = null;
        }

        if (state.diagram.draggedNode) {
            state.diagram.dragNode(null);
        }
    },
};
