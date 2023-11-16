<script lang="ts">
    import { createEventDispatcher, getContext } from "svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import { SearchMode, type DataModel } from "../deps/helveg-diagram.ts";
    import type { Readable } from "svelte/store";

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
    ];
    let selectedSearchMode = SearchMode.Contains;
    let searchText: string = "";

    let model = getContext<Readable<DataModel>>("model");
    export let results: string[] = [];
</script>

<Panel name="Search" indent={false} id={AppPanels.Search}>
    <Subpanel>
        <form
            on:submit|preventDefault={() =>
                dispatch("highlight", {
                    searchText: searchText,
                    searchMode: selectedSearchMode,
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
    <Subpanel name="Results">
        <div class="flex flex-col">
            {#each results as result}
            <div class="flex flex-row">
                <span>{$model.data?.nodes[result].name}</span>
                <span>{$model.data?.nodes[result].kind}</span>
            </div>
            {/each}
        </div>
    </Subpanel>
    {/if}
</Panel>
