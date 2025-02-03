<script lang="ts">
    import { getContext } from "svelte";
    import { OP_COLLAPSE_ALL, OP_DIG_IN, OP_DIG_OUT, OP_EXPAND_ALL } from "../operations/op-toggle.ts";
    import { type IExplorerState } from "../explorer-state.ts";
    import { ModifierFlags, OperationScope } from "../operations/executor.ts";
    import Icon from "./Icon.svelte";
    import { FALLBACK_ICON } from "@cafour/helveg-diagram";

    const state = getContext<IExplorerState>("state");

    const operations = [OP_DIG_IN, OP_EXPAND_ALL, OP_DIG_OUT, OP_COLLAPSE_ALL];

    let className: string | undefined;
    export { className as class };
</script>

<div class="toolbar {className}">
    {#each operations as operation (operation.id)}
        <button
            class="button-icon"
            on:click={async () =>
                await state.operationExecutor.triggerManually(operation, undefined, {
                    shouldBeginExecute: true,
                    shouldEndExecute: true,
                })}
        >
            <Icon name={operation.icon ?? FALLBACK_ICON.name} title={operation.hint} />
        </button>
    {/each}
</div>
