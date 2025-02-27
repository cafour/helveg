<script lang="ts">
    import { createEventDispatcher, getContext } from "svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import ResizingTextarea from "./ResizingTextarea.svelte";
    import { SearchMode, type DataModel, Diagram, type IFilterBuilderEntry } from "../deps/helveg-diagram.ts";
    import type { Readable } from "svelte/store";
    import FilterBuilder from "./FilterBuilder.svelte";
    import NodeIcon from "./NodeIcon.svelte";
    import Hint from "./Hint.svelte";
    import ButtonStretch from "./ButtonStretch.svelte";

    let dispatch = createEventDispatcher();

    let searchModes = [
        {
            value: SearchMode.Contains,
            icon: AppIcons.ContainsMode,
            hint: "Full-text case-insensitive in node names.",
        },
        {
            value: SearchMode.Regex,
            icon: AppIcons.RegexMode,
            hint: "Search among node names using a regular expression.",
        },
        {
            value: SearchMode.JavaScript,
            icon: AppIcons.JavaScriptMode,
            hint: "Search among any node data using a JavaScript expression. All properties from the properties panel become variables of the expression.",
        },
    ];
    let selectedSearchMode = SearchMode.Contains;
    let searchText: string = "";
    let expandedOnly: boolean = false;
    let filterBuilder: IFilterBuilderEntry[] = [];

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
    <form
        on:submit|preventDefault|stopPropagation={() =>
            dispatch("highlight", {
                searchText: searchText,
                searchMode: selectedSearchMode,
                expandedOnly: expandedOnly,
                filterBuilder: filterBuilder,
            })}
    >
        <Subpanel class="noborder">
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
                <input type="checkbox" bind:checked={expandedOnly} />
                <span>Expanded only</span>
                <Hint text="Search only among the currently visible nodes." />
            </label>
        </Subpanel>
        <Subpanel
            name="Filters"
            hint="Filters that must ALL be true for a node to appear among the results."
            isCollapsible={false}
            class="noborder"
        >
            <FilterBuilder bind:filterBuilder />
        </Subpanel>
        <Subpanel>
            <div class="flex flex-row gap-4">
                <input type="submit" class="button-stretch primary" value="Highlight" />
                <ButtonStretch
                    class="primary"
                    on:click={() =>
                        dispatch("isolate", {
                            searchText: searchText,
                            searchMode: selectedSearchMode,
                            filterBuilder: filterBuilder,
                        })}
                    hint="Remove all nodes except for the ones matching the search criteria."
                >
                    Isolate
                </ButtonStretch>
            </div>
        </Subpanel>
    </form>
    <Subpanel name={"Results (" + results.length + ")"} indent={false} isCollapsible={false} class="noborder">
        <div class="flex flex-col">
            {#each nodeResults as node}
                <!-- svelte-ignore a11y-click-events-have-key-events -->
                <!-- svelte-ignore a11y-no-static-element-interactions -->
                <div
                    class="flex flex-row search-item"
                    class:selected={node.id == diagram.selectedNode}
                    on:click={() => dispatch("selected", node.id)}
                >
                    <NodeIcon node={node.id} />
                    <span>{node.name}</span>
                </div>
            {/each}
        </div>
    </Subpanel>
</Panel>
