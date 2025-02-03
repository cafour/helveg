import type { Coordinates } from "../deps/helveg-diagram.ts";
import type { IExplorerState } from "../explorer-state.ts";

export interface KeyboardShortcut {
    key: string;
    modifiers?: ModifierFlags;
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
    FIFTH = 4,
}

export enum ModifierFlags {
    NONE = 0,
    CONTROL = 1 << 0,
    ALT = 1 << 1,
    SHIFT = 1 << 2,
}

export interface MouseGesture {
    button: MouseButton;
    modifiers?: ModifierFlags;
    isDouble?: boolean;
}

export enum OperationScope {
    GLOBAL = 1 << 0,
    NODE = 1 << 1,
    STAGE = 1 << 2,
}

export type OperationEventType = keyof Omit<
    Operation<never>,
    "id" | "name" | "scopes" | "hint" | "icon" | "shortcut" | "gesture" | "beginExecute" | "endExecute" | "hidden"
>;

export interface OperationEvent {
    modifiers: ModifierFlags;
    type: OperationEventType;
}

export interface MouseOperationEvent extends OperationEvent {
    button: MouseButton;
    isDouble: boolean;
    coords: Coordinates;
    hasMoved: boolean;
}

export interface KeyOperationEvent extends OperationEvent {
    key: string;
}

export interface Operation<TContext> {
    readonly id: string;
    readonly name: string;
    readonly scopes: OperationScope;

    readonly hint?: string;
    readonly icon?: string;
    readonly shortcut?: KeyboardShortcut;
    readonly gesture?: MouseGesture;
    readonly hidden?: boolean;

    keyDown?: (state: IExplorerState, context: TContext, event: KeyOperationEvent) => void | Promise<void>;
    keyUp?: (state: IExplorerState, context: TContext, event: KeyOperationEvent) => void | Promise<void>;

    mouseDown?: (state: IExplorerState, context: TContext, event: MouseOperationEvent) => void | Promise<void>;
    mouseMove?: (state: IExplorerState, context: TContext, event: MouseOperationEvent) => void | Promise<void>;
    mouseUp?: (state: IExplorerState, context: TContext, event: MouseOperationEvent) => void | Promise<void>;
    mouseEnter?: (state: IExplorerState, context: TContext, event: MouseOperationEvent) => void | Promise<void>;
    mouseLeave?: (state: IExplorerState, context: TContext, event: MouseOperationEvent) => void | Promise<void>;

    beginExecute?: (state: IExplorerState, context: TContext, event: OperationEvent) => void | Promise<void>;
    endExecute?: (state: IExplorerState, context: TContext, event: OperationEvent) => void | Promise<void>;
}

export type GlobalOperation = Operation<undefined>;
export type NodeOperation = Operation<string>;
export type StageOperation = Operation<undefined>;

function toModifiers(event: MouseEvent | KeyboardEvent): ModifierFlags {
    return (
        (event.altKey ? ModifierFlags.ALT : ModifierFlags.NONE) |
        (event.ctrlKey ? ModifierFlags.CONTROL : ModifierFlags.NONE) |
        (event.shiftKey ? ModifierFlags.SHIFT : ModifierFlags.NONE)
    );
}

function satisfiesModifiers(flags: ModifierFlags | undefined, event: OperationEvent): boolean {
    return (event.modifiers ?? ModifierFlags.NONE) === (flags ?? ModifierFlags.NONE);
}

function satisfiesGesture(gesture: MouseGesture, event: MouseOperationEvent): boolean {
    if (gesture.button !== event.button) {
        return false;
    }

    if ((gesture.isDouble ?? false) !== (event.isDouble ?? false)) {
        return false;
    }

    if (!satisfiesModifiers(gesture.modifiers, event)) {
        return false;
    }

    return true;
}

function satisfiesShortcut(shortcut: KeyboardShortcut, event: KeyOperationEvent): boolean {
    if (shortcut.key !== event.key) {
        return false;
    }

    if (!satisfiesModifiers(shortcut.modifiers, event)) {
        return false;
    }
    return true;
}

function satisfiesEvent(operation: Operation<any>, event: OperationEvent): boolean {
    const keyEvent = (event as any)["key"] !== undefined ? (event as KeyOperationEvent) : undefined;
    const mouseEvent = (event as any)["button"] !== undefined ? (event as MouseOperationEvent) : undefined;
    if (operation.shortcut && keyEvent && satisfiesShortcut(operation.shortcut, keyEvent)) {
        return true;
    } else if (operation.gesture && mouseEvent && satisfiesGesture(operation.gesture, mouseEvent)) {
        return true;
    }
    return false;
}

export interface TriggerOptions {
    event?: Event;
    shouldBeginExecute?: boolean;
    shouldEndExecute?: boolean;
}

export class OperationExecutor {
    private _scopeMaps = new Map<OperationScope, Map<string, Operation<unknown>>>();

    private _mousePressed = new Set<MouseButton>();
    public get mousePressed(): Readonly<Set<MouseButton>> {
        return this._mousePressed;
    }

    private _keyPressed = new Set<string>();
    public get keyPressed(): Readonly<Set<string>> {
        return this._keyPressed;
    }

    private _hasMoved = false;
    public get hasMoved(): boolean {
        return this._hasMoved;
    }

    public get pressedModifiers(): ModifierFlags {
        return (
            (this.state.diagram.modifierKeyState.alt ? ModifierFlags.ALT : ModifierFlags.NONE) |
            (this.state.diagram.modifierKeyState.control ? ModifierFlags.CONTROL : ModifierFlags.NONE) |
            (this.state.diagram.modifierKeyState.shift ? ModifierFlags.SHIFT : ModifierFlags.NONE)
        );
    }

    constructor(private state: IExplorerState) {
        window.addEventListener("keydown", (e) => this.onKeyDown(e));
        window.addEventListener("keyup", (e) => this.onKeyUp(e));

        state.diagram.events.nodeDown.subscribe(async (e) => {
            this.mousePressed.add(MouseButton.MAIN);
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e.coords,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: false,
                type: "mouseDown",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.NODE, e.nodeId, operationEvent, {
                shouldBeginExecute: true,
            });
        });
        state.diagram.events.nodeUp.subscribe(async (e) => {
            this.mousePressed.delete(MouseButton.MAIN);
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e.coords,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: false,
                type: "mouseUp",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.NODE, e.nodeId, operationEvent, {
                shouldEndExecute: true,
            });
        });
        state.diagram.events.nodeDoubleClicked.subscribe(async (e) => {
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e.coords,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: true,
                type: "mouseDown",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.NODE, e.nodeId, operationEvent, {
                shouldBeginExecute: true,
            });
        });
        state.diagram.events.nodeMove.subscribe(async (e) => {
            this._hasMoved = this._mousePressed.size > 0;
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e.coords,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: true,
                type: "mouseMove",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.NODE, e.nodeId, operationEvent, {});
        });
        state.diagram.events.stageDown.subscribe(async (e) => {
            this.mousePressed.add(MouseButton.MAIN);
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: false,
                type: "mouseDown",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.STAGE, undefined, operationEvent, {
                shouldBeginExecute: true,
            });
        });
        state.diagram.events.stageUp.subscribe(async (e) => {
            this.mousePressed.delete(MouseButton.MAIN);
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: false,
                type: "mouseUp",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.STAGE, undefined, operationEvent, {
                shouldEndExecute: true,
            });
        });
        state.diagram.events.stageDoubleClicked.subscribe(async (e) => {
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: true,
                type: "mouseDown",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.STAGE, undefined, operationEvent, {
                shouldBeginExecute: true,
            });
        });
        state.diagram.events.stageMove.subscribe(async (e) => {
            this._hasMoved = this._mousePressed.size > 0;
            const operationEvent: MouseOperationEvent = {
                button: MouseButton.MAIN,
                coords: e,
                modifiers: this.pressedModifiers,
                hasMoved: this.hasMoved,
                isDouble: true,
                type: "mouseMove",
            };
            await this.trigger(OperationScope.GLOBAL | OperationScope.STAGE, undefined, operationEvent, {});
        });
    }

    public register<T>(operation: Operation<T>) {
        for (const scope of Object.values(OperationScope) as OperationScope[]) {
            if ((operation.scopes & scope) !== scope) {
                continue;
            }

            if (!this._scopeMaps.has(scope)) {
                this._scopeMaps.set(scope, new Map());
            }

            const scopeMap = this._scopeMaps.get(scope)!;
            if (scopeMap.has(operation.id)) {
                throw new Error(`Operation '${operation.id}' is already present in the '${scope}' scope.`);
            }

            scopeMap.set(operation.id, operation as Operation<unknown>);
        }
    }

    public getOperations<Op extends Operation<unknown>>(scopes: OperationScope): Op[] {
        return [...this._scopeMaps.entries()]
            .filter((s) => (s[0] & scopes) === s[0])
            .flatMap((s) => [...s[1].values()]) as Op[];
    }

    public async trigger<TContext>(
        scopes: OperationScope,
        context: TContext,
        event: OperationEvent,
        options: TriggerOptions
    ): Promise<void> {
        const ops = this.getOperations(scopes).filter((op) => satisfiesEvent(op, event));
        for (const op of ops) {
            if (op[event.type]) {
                this.state.logger.debug(`Operation '${op.name}' triggered (${event.type}).`);
                options.event?.preventDefault();
                await op[event.type]!(this.state, context, event as KeyOperationEvent & MouseOperationEvent);
            }

            if (options.shouldBeginExecute && op.beginExecute) {
                this.state.logger.debug(`Operation '${op.name}' triggered (beginExecute).`);
                options.event?.preventDefault();
                await op.beginExecute(this.state, context, event);
            }

            if (options.shouldEndExecute && op.endExecute) {
                this.state.logger.debug(`Operation '${op.name}' triggered (endExecute).`);
                options.event?.preventDefault();
                await op.endExecute(this.state, context, event);
            }
        }
    }

    private async onKeyDown(event: KeyboardEvent) {
        if (event.key === "F5") {
            // reloads happen and should be ignored since at that point the executor object is already destroyed anyway
            return;
        }

        if ((event.target as HTMLElement).tagName !== "BODY") {
            return;
        }

        this._keyPressed.add(event.key);

        const operationEvent: KeyOperationEvent = {
            key: event.key,
            type: "keyDown",
            modifiers: toModifiers(event),
        };
        await this.trigger(
            this.state.diagram.selectedNode ? OperationScope.GLOBAL | OperationScope.NODE : OperationScope.GLOBAL,
            this.state.diagram.selectedNode,
            operationEvent,
            {
                event: event,
                shouldBeginExecute: true,
            }
        );
    }

    private async onKeyUp(event: KeyboardEvent) {
        if (event.key === "F5") {
            // reloads happen and should be ignored since at that point the executor object is already destroyed anyway
            return;
        }

        if ((event.target as HTMLElement).tagName !== "BODY") {
            return;
        }

        this._keyPressed.delete(event.key);

        const operationEvent: KeyOperationEvent = {
            key: event.key,
            type: "keyUp",
            modifiers: toModifiers(event),
        };
        await this.trigger(
            this.state.diagram.selectedNode ? OperationScope.GLOBAL | OperationScope.NODE : OperationScope.GLOBAL,
            this.state.diagram.selectedNode,
            operationEvent,
            {
                event: event,
                shouldEndExecute: true,
            }
        );
    }
}
