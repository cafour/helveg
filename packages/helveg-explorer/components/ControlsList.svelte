<script lang="ts">
    import { getShortcutHint, type Operation } from "../operations/executor.ts";

    export let operations: Operation<any>[] = [];
</script>

<div class="flex flex-col gap-16">
    <div class="grid grid-cols-3">
        <strong class="border-b-1" style="border-color: var(--color-text);">&nbsp;</strong>
        <strong class="border-b-1" style="border-color: var(--color-text);">Keyboard</strong>
        <strong class="border-b-1" style="border-color: var(--color-text);">Mouse</strong>
    </div>
    {#each operations as op}
        <div class="grid grid-cols-3">
            <strong>{op.name}</strong>
            {#if op.shortcut != null}
                <div>
                    <span class="keycap">{getShortcutHint(op.shortcut)}</span>
                </div>
            {:else}
                <span>&nbsp;</span>
            {/if}

            {#if op.gesture != null}
                <span class="gesture">TODO</span>
            {:else}
                <span>&nbsp;</span>
            {/if}

            <span class="col-3 text-xs">
                {op.hint}
            </span>
        </div>
    {/each}
</div>
