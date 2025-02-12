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
    import FilterBuilderSelect, { type SelectItem } from "./FilterBuilderSelect.svelte";

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

    const selectItems: SelectItem[] = nodeKeyCategories.flatMap((c) =>
        c.properties.map((p) => {
            return {
                value: p,
                label: p,
                group: c.name,
            };
        }),
    );

    let entries = writable<IFilterBuilderEntry[]>([]);
    export let filterBuilder = [];
    $: filterBuilder = $entries;

    let newItem: string | undefined = undefined;
    function createEntry(e: CustomEvent) {
        if (newItem) {
            $entries = [
                ...$entries,
                {
                    property: newItem,
                    operation: FilterBuilderOperation.Equals,
                    value: nodeKeyTypes[newItem] === "string" ? "" : nodeKeyTypes[newItem] === "number" ? 0 : true,
                },
            ];
            newItem = undefined;
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

    function onFocus(e: CustomEvent) {
        console.log(e);
        const input = e.detail.target as HTMLElement;
        const inputRect = input.getBoundingClientRect();
        const offset = inputRect.top + inputRect.height;

        let container = input;
        while (container != null && !container.classList.contains("filter-builder-select")) {
            container = container.parentElement as HTMLElement;
        }

        container.style.setProperty("--list-max-height", `calc(100vh - ${Math.ceil(offset)}px - 1rem)`);
    }
</script>

{#each $entries as entry, i}
    <div class="flex flex-row">
        <FilterBuilderSelect
            items={selectItems}
            bind:value={entry.property}
            on:change={() => onPropertyChanged(i)}
            class="flex-grow-0 flex-shrink-0 w-auto"
            clearable={false}
            --width="9rem"
        />
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
    <FilterBuilderSelect items={selectItems} on:focus={onFocus} bind:value={newItem} on:change={createEntry} />
</div>
