<script lang="ts">
    import { createEventDispatcher, getContext } from "svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import {
        SearchMode,
        type DataModel,
        Diagram,
    } from "../deps/helveg-diagram.ts";
    import type { Readable } from "svelte/store";
    import Icon from "./Icon.svelte";

    let dispatch = createEventDispatcher();

    let searchModes = [
        {
            value: SearchMode.Contains,
            icon: AppIcons.ContainsMode,
        },
        {
            value: SearchMode.Regex,
            icon: AppIcons.RegexMode,
        },
        {
            value: SearchMode.JavaScript,
            icon: AppIcons.JavaScriptMode,
        },
        {
            value: SearchMode.FilterBuilder,
            icon: AppIcons.JavaScriptMode,
        },
    ];
    let selectedSearchMode = SearchMode.Contains;
    let searchText: string = "";
    let expandedOnly: boolean = false;

    let model = getContext<Readable<DataModel>>("model");
    let diagram = getContext<Diagram>("diagram");
    export let results: string[] = [];
    $: nodeResults = results
        .map((r) => {
            return { ...$model.data?.nodes[r]!, id: r };
        })
        .filter((n) => n && n.name)
        .sort((l, r) => l.name!.localeCompare(r.name!));
</script>

<Panel name="Search" indent={false} id={AppPanels.Search}>
    <Subpanel>
        <form
            on:submit|preventDefault={() =>
                dispatch("highlight", {
                    searchText: searchText,
                    searchMode: selectedSearchMode,
                    expandedOnly: expandedOnly
                })}
        >
            <div class="flex flex-row gap-4">
                <ResizingTextarea bind:value={searchText} class="monospace" />
                <RadioGroup
                    groupName="searchMode"
                    items={searchModes}
                    bind:selected={selectedSearchMode}
                    class="theme-light"
                />
            </div>
            <label>
                <input
                    type="checkbox"
                    bind:checked={expandedOnly}
                />
                <span>ExpandedOnly</span>
            </label>
            <input
                type="submit"
                class="button-stretch mt-8 primary"
                value="Highlight"
            />
            <button
                class="button-stretch mt-8 primary"
                on:click|preventDefault|stopPropagation={() =>
                    dispatch("isolate", {
                        searchText: searchText,
                        searchMode: selectedSearchMode,
                    })}
            >
                Isolate
            </button>
        </form>
    </Subpanel>
    {#if results.length > 0}
        <Subpanel name="Results" indent={false}>
            <div class="flex flex-col">
                {#each nodeResults as node}
                    <!-- svelte-ignore a11y-click-events-have-key-events -->
                    <!-- svelte-ignore a11y-no-static-element-interactions -->
                    <div
                        class="flex flex-row search-item"
                        class:selected={node.id == diagram.selectedNode}
                        on:click={() => dispatch("selected", node.id)}
                    >
                        <Icon name={diagram.nodeStylist(node).icon} />
                        <span>{node.name}</span>
                    </div>
                {/each}
            </div>
        </Subpanel>
    {/if}
</Panel>
