import type { Diagram } from "./deps/helveg-diagram.ts";
import type { IExplorerState } from "./explorer-state";
import { DEFAULT_OPERATIONS, OperationScope, type KeyboardShortcut, type MouseGesture, type Operation } from "./operation";

function isGesture(gesture: MouseGesture, event: MouseEvent): boolean {
    if (gesture.button !== event.button) {
        return false;
    }

    if (gesture.modifiers
        && (gesture.modifiers.alt !== event.altKey
            || gesture.modifiers.shift !== event.shiftKey
            || gesture.modifiers.control !== event.ctrlKey)) {
        return false;
    }

    return true;
}

function isShortcut(shortcut: KeyboardShortcut, event: KeyboardEvent): boolean {
    if (shortcut.key !== event.key) {
        return false;
    }

    if (shortcut.modifiers
        && (shortcut.modifiers.alt !== event.altKey
            || shortcut.modifiers.shift !== event.shiftKey
            || shortcut.modifiers.control !== event.ctrlKey)) {
        return false;
    }
    return true;
}

export class OperationExecutor {
    private _scopeMaps = new Map<OperationScope, Map<string, Operation<unknown>>>();

    constructor(private state: IExplorerState) {
        for (const defaultOp of DEFAULT_OPERATIONS) {
            if (!this._scopeMaps.has(defaultOp.scope)) {
                this._scopeMaps.set(defaultOp.scope, new Map());
            }

            const scopeMap = this._scopeMaps.get(defaultOp.scope)!;
            scopeMap.set(defaultOp.id, defaultOp);
        }
    }

    public attach(diagram: Diagram) {
        window.addEventListener("keydown", e => this.onKeyDown(diagram, e));
        window.addEventListener("keyup", e => this.onKeyUp(diagram, e));
    }

    private onKeyDown(diagram: Diagram, event: KeyboardEvent) {
        if (event.key === "F5") {
            return;
        }
        
        if ((event.target as HTMLElement).tagName !== "BODY") {
            return;
        }

        if (diagram.selectedNode) {
            this.executeNodeKeyDown(diagram.selectedNode, event);
        } else {
            this.executeGlobalKeyDown(event);
        }
    }

    private onKeyUp(diagram: Diagram, event: KeyboardEvent) {
        if (event.key === "F5") {
            return;
        }
        
        if ((event.target as HTMLElement).tagName !== "BODY") {
            return;
        }

        if (diagram.selectedNode) {
            this.executeNodeKeyUp(diagram.selectedNode, event);
        } else {
            this.executeGlobalKeyUp(event);
        }
    }

    public getOperations<Op extends Operation<unknown>>(scope: OperationScope)
        : Readonly<Map<string, Op>> {

        return <Map<string, Op>>this._scopeMaps.get(scope)!;
    }

    public executeMouseDown<TContext>(operation: Operation<TContext>, context: TContext, event: MouseEvent) {
        if (operation.mouseDown && (!operation.gesture || isGesture(operation.gesture, event))) {
            operation.mouseDown(this.state, context, event);
        }
    }

    public executeMouseUp<TContext>(operation: Operation<TContext>, context: TContext, event: MouseEvent) {
        if (operation.mouseUp && (!operation.gesture || isGesture(operation.gesture, event))) {
            operation.mouseUp(this.state, context, event);
        }
    }

    public executeKeyUp<TContext>(operation: Operation<TContext>, context: TContext, event: KeyboardEvent) {
        if (operation.keyUp && (!operation.shortcut || isShortcut(operation.shortcut, event))) {
            operation.keyUp(this.state, context, event);
        }
    }

    public executeKeyDown<TContext>(operation: Operation<TContext>, context: TContext, event: KeyboardEvent) {
        if (operation.keyDown && (!operation.shortcut || isShortcut(operation.shortcut, event))) {
            operation.keyDown(this.state, context, event);
        }
    }

    public executeGlobalMouseDown(event: MouseEvent) {
        this.getOperations(OperationScope.GLOBAL)
            .forEach(op => this.executeMouseDown<never>(op, undefined as never, event))
    }

    public executeGlobalMouseUp(event: MouseEvent) {
        this.getOperations(OperationScope.GLOBAL)
            .forEach(op => this.executeMouseUp<never>(op, undefined as never, event))
    }

    public executeGlobalKeyDown(event: KeyboardEvent) {
        this.getOperations(OperationScope.GLOBAL)
            .forEach(op => this.executeKeyDown<never>(op, undefined as never, event))
    }

    public executeGlobalKeyUp(event: KeyboardEvent) {
        this.getOperations(OperationScope.GLOBAL)
            .forEach(op => this.executeKeyUp<never>(op, undefined as never, event))
    }

    public executeNodeMouseDown(nodeId: string, event: MouseEvent) {
        this.getOperations(OperationScope.NODE)
            .forEach(op => this.executeMouseDown<string>(op, nodeId, event))
    }

    public executeNodeMouseUp(nodeId: string, event: MouseEvent) {
        this.getOperations(OperationScope.NODE)
            .forEach(op => this.executeMouseUp<string>(op, nodeId, event))
    }

    public executeNodeKeyDown(nodeId: string, event: KeyboardEvent) {
        this.getOperations(OperationScope.NODE)
            .forEach(op => this.executeKeyDown<string>(op, nodeId, event))
    }

    public executeNodeKeyUp(nodeId: string, event: KeyboardEvent) {
        this.getOperations(OperationScope.NODE)
            .forEach(op => this.executeKeyUp<string>(op, nodeId, event))
    }
}
