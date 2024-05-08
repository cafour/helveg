import type { IExplorerState } from "./explorer-state";
import { DEFAULT_OPERATIONS, OperationScope, type IOperation } from "./operation";

export class OperationExecutor {
    private _scopeMaps = new Map<OperationScope, Map<string, IOperation<unknown>>>();

    constructor(private state: IExplorerState) {
        for (const defaultOp of DEFAULT_OPERATIONS) {
            if (!this._scopeMaps.has(defaultOp.scope)) {
                this._scopeMaps.set(defaultOp.scope, new Map());
            }

            const scopeMap = this._scopeMaps.get(defaultOp.scope)!;
            scopeMap.set(defaultOp.id, defaultOp);
        }
    }

    public getOperations<Operation extends IOperation<unknown>>(scope: OperationScope)
        : Readonly<Map<string, Operation>> {

        return <Map<string, Operation>>this._scopeMaps.get(scope)!;
    }
    
    public executeClick<TContext>(operation: IOperation<TContext>, context: TContext, event: MouseEvent) {
        if (operation.mouseDown) {
            operation.mouseDown(this.state, context, event);
        }

        if (operation.mouseUp) {
            operation.mouseUp(this.state, context, event);
        }
    }
}
