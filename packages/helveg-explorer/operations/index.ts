import type { IExplorerState } from "../explorer-state.ts";
import { OperationExecutor } from "./executor.ts";
import { OP_GLOBAL_DESELECT, OP_INSPECT, OP_STAGE_DESELECT } from "./op-inspect.ts";
import { OP_AUTOLAYOUT, OP_LAYOUT, OP_REFRESH } from "./op-layout.ts";
import { OP_MOVE } from "./op-move.ts";
import { OP_REMOVE } from "./op-remove.ts";
import { OP_TOGGLE, OP_DIG_IN, OP_DIG_OUT, OP_COLLAPSE_ALL, OP_EXPAND_ALL } from "./op-toggle.ts";

export * from "./executor.ts";
export * from "./op-move.ts";
export * from "./op-inspect.ts";
export * from "./op-toggle.ts";
export * from "./op-remove.ts";
export * from "./op-layout.ts";

export function createDefaultExecutor(state: IExplorerState): OperationExecutor {
    const executor = new OperationExecutor(state);
    executor.register(OP_MOVE);
    executor.register(OP_INSPECT);
    executor.register(OP_GLOBAL_DESELECT);
    executor.register(OP_STAGE_DESELECT);
    executor.register(OP_TOGGLE);
    executor.register(OP_DIG_IN);
    executor.register(OP_DIG_OUT);
    executor.register(OP_EXPAND_ALL);
    executor.register(OP_COLLAPSE_ALL);
    executor.register(OP_REMOVE);
    executor.register(OP_LAYOUT);
    executor.register(OP_REFRESH);
    executor.register(OP_AUTOLAYOUT);
    return executor;
}
