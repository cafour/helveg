import type { ModifierKeyState } from "@cafour/helveg-diagram";
import { AppIcons } from "./const";
import type { IExplorerState } from "./explorer-state";

export interface KeyboardShortcut {
    key: string,
    modifiers?: ModifierKeyState
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

export interface MouseGesture {
    button: MouseButton,
    modifiers?: ModifierKeyState
}

export enum OperationScope {
    GLOBAL = "global",
    TOOLBOX = "toolbox",
    NODE = "node"
}

export interface Operation<TContext> {
    id: string,
    name: string;
    hint?: string;
    icon?: string,
    scope: OperationScope
    shortcut?: KeyboardShortcut,
    gesture?: MouseGesture,

    keyDown?: (state: IExplorerState, context: TContext, event: KeyboardEvent) => void | Promise<void>,
    keyUp?: (state: IExplorerState, context: TContext, event: KeyboardEvent) => void | Promise<void>

    mouseDown?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
    mouseMove?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
    mouseUp?: (state: IExplorerState, context: TContext, event: MouseEvent) => void | Promise<void>,
}

export type GlobalOperation = Operation<never>;
export type NodeOperation = Operation<string>;

export const DEFAULT_OPERATIONS: Array<Operation<unknown>> = [
    <NodeOperation>{
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
    } as Operation<unknown>,
    <GlobalOperation>{
        id: "toggle-move-tool",
        name: "Toggle the Move tool",
        hint: "Selects the Move tool from the Toolbox.",
        shortcut: {
            key: "m"
        },
        icon: AppIcons.MoveTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.selectedTool.set("mouse");
        },
    } as Operation<unknown>,
    <NodeOperation>{
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
            state.selectedNode.set(node ?? null);
        },
        mouseUp(state, node) {
            state.selectedNode.set(node ?? null);
        }
    } as Operation<unknown>,
    <GlobalOperation>{
        id: "toggle-show-properties-tool",
        name: "Toggle the Show properties tool",
        hint: "Toggle the Show properties tool from the Toolbox",
        shortcut: {
            key: "p"
        },
        icon: AppIcons.ShowPropertiesTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.selectedTool.set("show-properties");
        },
    } as Operation<unknown>,
    <NodeOperation>{
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
    } as Operation<unknown>,
    <GlobalOperation>{
        id: "tool-toggle",
        name: "Switch to the Toggle tool",
        hint: "Collapses or expands nodes.",
        shortcut: {
            key: "t"
        },
        icon: AppIcons.ToggleTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.selectedTool.set("toggle");
        },
    } as Operation<unknown>,
    <NodeOperation>{
        id: "remove",
        name: "Remove",
        hint: "Removes nodes and (optionally) its descendants.",
        shortcut: {
            key: "Delete"
        },
        gesture: {
            button: MouseButton.MAIN
        },
        icon: AppIcons.RemoveTool,
        scope: OperationScope.NODE,

        async keyDown(state, node) {
            if (!node) {
                state.logger.warn("Select a node to be removed.");
                return;
            }

            await state.diagram.remove(node);
        },
        async mouseDown(state, node) {
            if (!node) {
                state.logger.warn("Select a node to be removed.");
                return;
            }

            await state.diagram.remove(node);
        }
    } as Operation<unknown>,
    <GlobalOperation>{
        id: "tool-remove",
        name: "Switch to the Remove tool",
        hint: "Removes nodes and (optionally) its descendants.",
        shortcut: {
            key: "r"
        },
        icon: AppIcons.RemoveTool,
        scope: OperationScope.TOOLBOX,

        keyUp(state) {
            state.selectedTool.set("toggle");
        },
    } as Operation<unknown>,
]
