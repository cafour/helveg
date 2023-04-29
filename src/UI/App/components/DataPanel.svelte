<script lang="ts">
    import { createEventDispatcher } from "svelte/internal";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { SearchMode, type DataOptions } from "model/options";
    import { AppIcons } from "model/const";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import type { VisualizationModel } from "model/visualization";

    let dispatch = createEventDispatcher();

    export let dataOptions: DataOptions;
    export let model: VisualizationModel;

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
    let selectedSearchMode: SearchMode = SearchMode.Contains;
    let searchText: string = "";

    $: relations = model ? Object.keys(model.multigraph.relations).sort() : [];
</script>

<Panel name="Data" indent={false}>
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
                    class="light"
                />
            </div>
            <input
                type="submit"
                class="button-stretch mt-8"
                value="Highlight"
            />
            <button
                class="button-stretch mt-8"
                on:click|preventDefault={() =>
                    dispatch("isolate", {
                        searchText: searchText,
                        searchMode: selectedSearchMode,
                    })}
            >
                Isolate
            </button>
        </form>
    </Subpanel>
    <Subpanel name="IncludedRelations">
        {#each relations as relation}
            <label>
                <input
                    type="checkbox"
                    bind:group={dataOptions.selectedRelations}
                    value={relation}
                />
                {relation}
            </label>
        {/each}
    </Subpanel>
    <!-- <Subpanel name="IncludedKinds">
        {#each dataOptions.kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={dataOptions.selectedKinds}
                    value={kind}
                />
                <Icon name={dataOptions.defaultIcons[kind] ?? dataOptions.fallbackIcon} />
                {kind}
            </label>
        {/each}
    </Subpanel> -->
</Panel>
