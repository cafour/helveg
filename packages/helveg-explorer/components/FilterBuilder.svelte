<script lang="ts">
    import {
        CSHARP_PROP_CATEGORIES,
        FilterBuilderOperation,
        OPERATORS_BY_TYPE,
        type IFilterBuilderEntry,
        type PropCategory,
    } from "@cafour/helveg-diagram";
    import { getContext } from "svelte";
    import type { IExplorerState } from "../explorer-state";
    import { writable } from "svelte/store";

    const nodeKeyTypes = getContext<IExplorerState>("state").diagram.nodeKeyTypes;
    const nodeKeys = getContext<IExplorerState>("state").diagram.nodeKeys.filter(
        (k) =>
            !k.startsWith("$") &&
            (nodeKeyTypes[k] == "string" || nodeKeyTypes[k] == "number" || nodeKeyTypes[k] == "boolean"),
    );
    const miscKeys: string[] = [];
    const nodeKeyReverseLookup: Record<string, string> = {};
    for (const cat of CSHARP_PROP_CATEGORIES) {
        for (const prop of cat.properties) {
            nodeKeyReverseLookup[prop] = cat.name;
        }
    }

    const nodeKeyGroups: Record<string, string[]> = {};
    for (const nodeKey of nodeKeys) {
        if (nodeKeyReverseLookup[nodeKey] != null) {
            const cat = nodeKeyReverseLookup[nodeKey];
            if (nodeKeyGroups[cat] == null) {
                nodeKeyGroups[cat] = [];
            }
            nodeKeyGroups[cat].push(nodeKey);
        } else {
            miscKeys.push(nodeKey);
        }
    }
    const categoryIndices = CSHARP_PROP_CATEGORIES.map((c) => c.name);
    const nodeKeyCategories: PropCategory[] = Object.entries(nodeKeyGroups)
        .sort(([a], [b]) => categoryIndices.indexOf(a) - categoryIndices.indexOf(b))
        .map(([group, props]) => {
            const canon = CSHARP_PROP_CATEGORIES.find((c) => c.name === group)?.properties ?? [];
            return {
                name: group,
                properties: props.sort((a, b) => canon.indexOf(a) - canon.indexOf(b)),
            };
        });
    nodeKeyCategories.push({
        name: "Miscellaneous",
        properties: miscKeys.sort(),
    });

    let entries = writable<IFilterBuilderEntry[]>([]);
    export let filterBuilder = [];
    $: filterBuilder = $entries;

    function createEntry(e: Event & { currentTarget: EventTarget & HTMLSelectElement }) {
        if (e.currentTarget.value) {
            $entries = [
                ...$entries,
                {
                    property: e.currentTarget.value,
                    operation: FilterBuilderOperation.Equals,
                    value:
                        nodeKeyTypes[e.currentTarget.value] === "string"
                            ? ""
                            : nodeKeyTypes[e.currentTarget.value] === "number"
                              ? 0
                              : true,
                },
            ];
            e.currentTarget.value = "";
        }
    }

    function onPropertyChanged(i: number) {
        if (nodeKeyTypes[$entries[i].property] === "boolean") {
            $entries[i].operation = FilterBuilderOperation.Equals;
        }
    }

    function removeEntry(index: number) {
        $entries = $entries.toSpliced(index, 1);
    }
</script>

{#each $entries as entry, i}
    <div class="flex flex-row">
        <select
            bind:value={entry.property}
            on:change={() => onPropertyChanged(i)}
            class="flex-grow-0 flex-shrink-0 w-auto text-xs"
        >
            {#each nodeKeyCategories as cat}
                <optgroup label={cat.name}>
                    {#each cat.properties as prop}
                        <option value={prop}>{prop}</option>
                    {/each}
                </optgroup>
            {/each}
        </select>
        <select
            bind:value={entry.operation}
            disabled={nodeKeyTypes[entry.property] == "boolean"}
            class="flex-grow-0 flex-shrink-0 w-64 text-xs"
        >
            {#each OPERATORS_BY_TYPE[nodeKeyTypes[entry.property]] as operator}
                <option value={operator}>{operator}</option>
            {/each}
        </select>
        {#if nodeKeyTypes[entry.property] === "boolean"}
            <select
                class="flex-grow-1 flex-shrink-0 w-auto text-xs"
                on:change={(e) => (entry.value = e.currentTarget.value === "true" ? true : false)}
            >
                <option value="true">true</option>
                <option value="false">false</option>
            </select>
        {:else if nodeKeyTypes[entry.property] === "number"}
            <input bind:value={entry.value} type="number" class="flex-grow-1 flex-shrink-0 w-0 text-xs" size="1" />
        {:else}
            <input bind:value={entry.value} class="flex-grow-1 flex-shrink-0 w-0 text-xs" size="1" />
        {/if}
        <button on:click={(e) => e.type === "click" && removeEntry(i)} type="button" class="button-icon primary">
            ✕
        </button>
    </div>
{/each}

<div>
    <select on:change={createEntry} class="text-xs">
        <option value="">Select a property</option>
        {#each nodeKeyCategories as cat}
            <optgroup label={cat.name}>
                {#each cat.properties as prop}
                    <option value={prop}>{prop}</option>
                {/each}
            </optgroup>
        {/each}
    </select>
</div>
