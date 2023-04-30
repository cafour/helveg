<script lang="ts">
    import { createEventDispatcher } from "svelte/internal";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { SearchMode } from "model/options";
    import { AppIcons } from "model/const";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import { dataOptions, model } from "./App.svelte";
    import CSharpKindsSubpanel from "./CSharpKindsSubpanel.svelte";

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
    let selectedSearchMode: SearchMode = SearchMode.Contains;
    let searchText: string = "";
    let allCheckbox: HTMLInputElement;

    $: relations = $model
        ? Object.keys($model.multigraph.relations).sort()
        : [];
    $: if (allCheckbox) {
        allCheckbox.indeterminate = $dataOptions.selectedRelations.length > 0
            && $dataOptions.selectedRelations.length < relations.length;
    };

    function toggleAllRelations() {
        if ($dataOptions.selectedRelations.length > 0) {
            $dataOptions.selectedRelations = [];
        } else {
            $dataOptions.selectedRelations = relations;
        }
    }
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
        <label>
            <input
                bind:this={allCheckbox}
                type="checkbox"
                on:click={toggleAllRelations}
                checked={$dataOptions.selectedRelations.length > 0}
            />
            all
        </label>
        {#each relations as relation}
            <label>
                <input
                    type="checkbox"
                    bind:group={$dataOptions.selectedRelations}
                    value={relation}
                />
                {relation}
            </label>
        {/each}
    </Subpanel>
    <CSharpKindsSubpanel />
</Panel>
