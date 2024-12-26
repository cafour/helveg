<script lang="ts">
    import {
        FilterBuilderOperation,
        type IFilterBuilderEntry,
    } from "@cafour/helveg-diagram";
    import { getContext } from "svelte";
    import type { IExplorerState } from "../explorer-state";
    import { writable } from "svelte/store";

    const nodeKeys =
        getContext<IExplorerState>("state").diagram.nodeKeys.filter(k => !k.startsWith("$")).toSorted();
    const operators = Object.values(FilterBuilderOperation);
    let entries = writable<IFilterBuilderEntry[]>([]);

    function createEntry(
        e: Event & { currentTarget: EventTarget & HTMLSelectElement },
    ) {
        if (e.currentTarget.value) {
            $entries = [...$entries, {
                property: e.currentTarget.value,
                operation: FilterBuilderOperation.Equals,
                value: "",
            }];
            e.currentTarget.value = "";
        }
    }
    
    function removeEntry(index: number) {
        $entries = $entries.toSpliced(index, 1);
    }
</script>

{#each $entries as entry, i}
    <div class="flex flex-row">
        <select bind:value={entry.property} class="flex-grow-0 flex-shrink-0 w-auto text-xs">
            {#each nodeKeys as key}
                <option value={key}>{key}</option>
            {/each}
        </select>
        <select bind:value={entry.operation} class="flex-grow-0 flex-shrink-0 w-auto text-xs">
            {#each operators as operator}
                <option value={operator}>{operator}</option>
            {/each}
        </select>
        <input bind:value={entry.value} class="flex-grow-1 flex-shrink-0 w-auto text-xs" size="5"/>
        <button on:click={() => removeEntry(i)} class="button-icon primary">
            ✕
        </button>
    </div>
{/each}

<div>
    <select on:change={createEntry} class="text-sm">
        <option value="">Select a property</option>
        {#each nodeKeys as key}
            <option value={key}>{key}</option>
        {/each}
    </select>
</div>
