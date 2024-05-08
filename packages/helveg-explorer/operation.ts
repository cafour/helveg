import { AppIcons } from "./const";
import type { IExplorerState } from "./explorer-state";

export interface IKeyboardShortcut {
    key: string,
    ctrlKey?: boolean,
    shiftKey?: boolean,
    altKey?: boolean
}

/**
 * Mouse buttons.
 * Based on https://developer.mozilla.org/en-US/docs/Web/API/MouseEvent/button.
 */
export enum MouseButton {
    /**
     * For right-handed users, typically left mouse button.
     */
    MAIN = 0,

    /**
     * Typically middle mouse button.
     */
    AUXILILARY = 1,

    /**
     * For right-handed users, typically right mouse button.
     */
    SECONDARY = 2,

    /**
     * Typically "Browser back" button.
     */
    FOURTH = 3,

    /**
     * Typically "Browser forward" button.
     */
    FIFTH = 4
}

export interface IMouseGesture {
    button: MouseButton,
    altKey?: boolean,
}

export enum OperationScope {
    GLOBAL = 1 << 0,
    TOOLBOX = 1 << 1,
    NODE = 1 << 2
}

export interface IOperation<TContext> {
    id: string,
    name: string;
    hint?: string;
    icon?: string,
    scope: OperationScope
    shortcut?: IKeyboardShortcut,
    gesture?: IMouseGesture,

    keyDown?: (state: IExplorerState, context: TContext, event: KeyboardEvent) => void | Promise<void>,
    keyUp?: (state: IExplorerState, context: TContext, event: KeyboardEvent) => void | Promise<void>

    mouseDown?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
    mouseMove?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
    mouseUp?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
}

export type IGlobalOperation = IOperation<undefined>;
export type INodeOperation = IOperation<string | undefined>;

export const DEFAULT_OPERATIONS: Array<IOperation<any>> = [
    <INodeOperation>{
        id: "move",
        name: "Move",
        hint: "Moves a node.",
        icon: AppIcons.MoveTool,
        scope: OperationScope.NODE,
        shortcut: {
            key: "m"
        },
        gesture: {
            button: MouseButton.MAIN
        },

        keyDown(state, node) {
            // TODO: state.diagram.draggedNode = node;
        },
        keyUp(state, node) {
            // TODO: state.diagram.draggedNode = null;
        },

        mouseDown(state, node) {
            // TODO: state.diagram.draggedNode = node;
        },
        mouseMove(state, node) {
            // TODO: Move dragging logic out of Diagram.
        },
        mouseUp(state, node) {
            // TODO: state.diagram.draggedNode = null;
        }
    },
    <IGlobalOperation>{
        id: "toggle-move-tool",
        name: "Toggle the Move tool",
        hint: "Selects the Move tool from the Toolbox.",
        shortcut: {
            key: "m"
        },
        icon: AppIcons.MoveTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.mouseOperation.set("mouse");
        },
    },
    <INodeOperation>{
        id: "show-properties",
        name: "Show properties",
        hint: "Show properties of the selected node.",
        shortcut: {
            key: "p"
        },
        gesture: {
            button: MouseButton.MAIN
        },
        icon: AppIcons.ShowPropertiesTool,
        scope: OperationScope.NODE,

        keyUp(state, node) {
            state.selectedNode.set(node);
        },
        mouseUp(state, node) {
            state.selectedNode.set(node);
        }
    },
    <IGlobalOperation>{
        id: "toggle-show-properties-tool",
        name: "Toggle the Show properties tool",
        hint: "Tohhle the Show properties tool from the Toolbox",
        shortcut: {
            key: "p"
        },
        icon: AppIcons.ShowPropertiesTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.mouseOperation.set("show-properties");
        },
    },
    <INodeOperation>{
        id: "toggle",
        name: "Toggle",
        hint: "Collapses or expands nodes.",
        shortcut: {
            key: "t"
        },
        gesture: {
            button: MouseButton.MAIN
        },
        icon: AppIcons.ToggleTool,
        scope: OperationScope.NODE,

        async keyUp(state, node) {
            if (!node) {
                state.logger.warn("Select a node to Toggle.");
                return;
            }

            await state.diagram.toggleNode(node);
        },
        async mouseUp(state, node) {
            if (!node) {
                state.logger.warn("Select a node to Toggle.");
                return;
            }

            await state.diagram.toggleNode(node);
        }
    },
    <IGlobalOperation>{
        id: "tool-toggle",
        name: "Switch to the Toggle tool",
        hint: "Collapses or expands nodes.",
        shortcut: {
            key: "t"
        },
        icon: AppIcons.ToggleTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.mouseOperation.set("toggle");
        },
    },
    <INodeOperation>{
        id: "cut",
        name: "Cut",
        hint: "Cuts nodes and (optionally) its descendants.",
        shortcut: {
            key: "c"
        },
        gesture: {
            button: MouseButton.MAIN
        },
        icon: AppIcons.CutTool,
        scope: OperationScope.NODE,

        async keyUp(state, node) {
            if (!node) {
                state.logger.warn("Select a node to Cut.");
                return;
            }

            await state.diagram.cut(node);
        },
        async mouseUp(state, node) {
            if (!node) {
                state.logger.warn("Select a node to Cut.");
                return;
            }

            await state.diagram.cut(node);
        }
    },
    <IGlobalOperation>{
        id: "tool-cut",
        name: "Switch to the Cut tool",
        hint: "Cuts nodes and (optionally) its descendants.",
        shortcut: {
            key: "c"
        },
        icon: AppIcons.CutTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.mouseOperation.set("toggle");
        },
    },
]
