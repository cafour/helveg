<script lang="ts">
    import { getContext } from "svelte";
    import { OP_COLLAPSE_ALL, OP_DIG_IN, OP_DIG_OUT, OP_EXPAND_ALL } from "../operations/op-toggle.ts";
    import { type IExplorerState } from "../explorer-state.ts";
    import { getShortcutHint, OP_AUTOLAYOUT, OP_REFRESH, type Operation } from "../operations/index.ts";
    import ButtonIcon from "./ButtonIcon.svelte";

    const state = getContext<IExplorerState>("state");

    const operations = [OP_AUTOLAYOUT, OP_REFRESH, OP_DIG_IN, OP_EXPAND_ALL, OP_DIG_OUT, OP_COLLAPSE_ALL];

    let className: string | undefined;
    export { className as class };

    export let style: string | undefined = undefined;

    async function trigger(operation: Operation<any>) {
        (document.activeElement as HTMLElement)?.blur();
        await state.operationExecutor.triggerManually(operation, undefined, {
            shouldBeginExecute: true,
            shouldEndExecute: true,
        });
    }
</script>

<div class="toolbar {className}" {style}>
    {#each operations as operation (operation.id)}
        <ButtonIcon
            class="button-icon"
            on:click={async (e) => {
                trigger(operation);
            }}
            icon={operation.icon}
            name={operation.name}
            hint={operation.hint}
            shortcut={getShortcutHint(operation.shortcut)}
        />
    {/each}
</div>
