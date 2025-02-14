<script lang="ts">
    import { getShortcutHint, type Operation } from "../operations/executor.ts";
    import GestureHint from "./GestureHint.svelte";

    export let name: string = "Operations";
    export let operations: Operation<any>[] = [];
</script>

<div class="flex flex-col gap-16">
    <div class="grid grid-cols-3">
        <strong class="border-b-1 extrabold" style="border-color: var(--color-text);">{name}</strong>
        <span class="border-b-1" style="border-color: var(--color-text);">Keyboard</span>
        <span class="border-b-1" style="border-color: var(--color-text);">Mouse</span>
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
                <div>
                    <GestureHint gesture={op.gesture} />
                </div>
            {:else}
                <span>&nbsp;</span>
            {/if}

            <span class="col-3 text-xs">
                {op.hint}
            </span>
        </div>
    {/each}
</div>
