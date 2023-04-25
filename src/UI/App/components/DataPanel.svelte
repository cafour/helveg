<script lang="ts">
    import { createEventDispatcher } from "svelte/internal";
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { SearchMode, type DataOptions } from "model/options";
    import { AppIcons } from "model/const";
    import ResizingTextarea from "./ResizingTextarea.svelte";

    export let dataOptions: DataOptions;

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

    let dispatch = createEventDispatcher();
</script>

<Panel name="Data" indent={false}>
    <div class="indent">
        <button on:click={() => dispatch("reset")} class="button-stretch">
            Reset
        </button>
    </div>
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
