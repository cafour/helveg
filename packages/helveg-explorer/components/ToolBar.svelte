<script lang="ts">
    import { getContext } from "svelte";
    import { OP_COLLAPSE_ALL, OP_DIG_IN, OP_DIG_OUT, OP_EXPAND_ALL } from "../operations/op-toggle.ts";
    import { type IExplorerState } from "../explorer-state.ts";
    import { OP_AUTOLAYOUT, OP_REFRESH } from "../operations/index.ts";
    import ButtonIcon from "./ButtonIcon.svelte";

    const state = getContext<IExplorerState>("state");

    const operations = [OP_AUTOLAYOUT, OP_REFRESH, OP_DIG_IN, OP_EXPAND_ALL, OP_DIG_OUT, OP_COLLAPSE_ALL];

    let className: string | undefined;
    export { className as class };
</script>

<div class="toolbar {className}">
    {#each operations as operation (operation.id)}
        <ButtonIcon
            class="button-icon"
            on:click={async () =>
                await state.operationExecutor.triggerManually(operation, undefined, {
                    shouldBeginExecute: true,
                    shouldEndExecute: true,
                })}
            icon={operation.icon}
            name={operation.name}
            hint={operation.hint}
        />
    {/each}
</div>
