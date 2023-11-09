<script lang="ts">
    import { createEventDispatcher, getContext } from "svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { type DataOptions } from "../options.ts";
    import { AppIcons, AppPanels } from "../const.ts";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import ToggleAllCheckbox from "./ToggleAllCheckbox.svelte";
    import type { SearchMode, VisualizationModel } from "../deps/helveg-diagram.ts";
    import type { Readable, Writable } from "svelte/store";

    let dispatch = createEventDispatcher();

    let searchModes = [
        {
            value: helveg.SearchMode.Contains,
            icon: AppIcons.ContainsMode,
        },
        {
            value: helveg.SearchMode.Regex,
            icon: AppIcons.RegexMode,
        },
        {
            value: helveg.SearchMode.JavaScript,
            icon: AppIcons.JavaScriptMode,
        },
    ];
    let selectedSearchMode = helveg.SearchMode.Contains;
    let searchText: string = "";

    let model = getContext<Readable<VisualizationModel>>("model");
    let dataOptions = getContext<Writable<DataOptions>>("dataOptions");
    
    $: relations = $model
        ? Object.keys($model.multigraph.relations).sort()
        : [];
</script>

<Panel name="Data" indent={false} id={AppPanels.Data}>
    <Subpanel>
        <button on:click={() => dispatch("refresh")} class="button-stretch">
            Refresh
        </button>
    </Subpanel>
    <Subpanel name="Search">
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
                class="button-stretch mt-8"
                value="Highlight"
            />
            <button
                class="button-stretch mt-8"
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
    <Subpanel name="Relations">
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>
            <ToggleAllCheckbox
                bind:selected={$dataOptions.selectedRelations}
                all={relations}
            />
            <span>all</span>
        </label>
        {#each relations as relation}
            <label>
                <input
                    type="checkbox"
                    bind:group={$dataOptions.selectedRelations}
                    value={relation}
                />
                <span>{relation}</span>
            </label>
        {/each}
    </Subpanel>
</Panel>
