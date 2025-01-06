<script lang="ts" context="module">
    export type TreeViewItem = HelvegForestItem & {
        isExpanded: Writable<boolean>;
        parent?: TreeViewItem;
        children?: TreeViewItem[];
    };
</script>

<script lang="ts">
    import { createEventDispatcher, getContext, onMount } from "svelte";
    import { AppPanels } from "../const";
    import Panel from "./Panel.svelte";
    import type { IExplorerState } from "../explorer-state";
    import {
        FALLBACK_NODE_ICON,
        getForest,
        getForestItems,
        type HelvegForestItem,
    } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";
    import {
        get,
        readable,
        writable,
        type Readable,
        type Writable,
    } from "svelte/store";
    import TreeViewEntry from "./TreeViewEntry.svelte";

    const state = getContext<IExplorerState>("state");
    const graph = state.graph;
    const layoutOptions = state.layoutOptions;

    let additionalClass: string | undefined = undefined;
    export { additionalClass as class };

    let items: TreeViewItem[] = [];
    $: {
        items = getForestItems(
            getForest($graph, $layoutOptions.tidyTree.relation ?? "declares"),
        ) as TreeViewItem[];
        for (const item of items) {
            item.isExpanded = writable(false);
        }
    }

    let isOpen = false;

    onMount(() => {
        isOpen = localStorage.getItem(`TreeView.isOpen`) === "true";
    });

    export function togglePanel() {
        isOpen = !isOpen;
        localStorage.setItem(`TreeView.isOpen`, isOpen ? "true" : "false");
    }

    let dispatch = createEventDispatcher();

    function onNodeClicked(nodeId: string | null) {
        dispatch("nodeClicked", { nodeId: nodeId });
        selectedNode = nodeId;
    }

    export let selectedNode: string | null = null;
</script>

<div class="tree-view {additionalClass} flex flex-row" class:open={isOpen}>
    <Panel
        id={AppPanels.TreeView}
        name="Tree View"
        class="flex-grow-1"
        indent={false}
    >
        {#if items.length > 0}
            {#each items as node}
                <TreeViewEntry
                    isSelected={node.id === selectedNode}
                    {node}
                    {onNodeClicked}
                />
            {/each}
        {:else}
            <i
                >The displayed graph is empty or no relation is selected. Use
                the Layout panel to refresh the graph.</i
            >
        {/if}
    </Panel>
    <button class="toggle" on:click={togglePanel}>
        <Icon
            title={isOpen ? "Close" : "Open"}
            name={isOpen ? "vscode:chevron-left" : "vscode:chevron-right"}
        />
    </button>
</div>
