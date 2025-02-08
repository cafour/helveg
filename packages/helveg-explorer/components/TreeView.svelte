<script lang="ts" context="module">
    export type TreeViewItem = HelvegForestItem & {
        parent?: TreeViewItem;
        children?: TreeViewItem[];
    };
</script>

<script lang="ts">
    import { createEventDispatcher, getContext, onMount } from "svelte";
    import { AppPanels } from "../const";
    import Panel from "./Panel.svelte";
    import type { IExplorerState } from "../explorer-state";
    import { getForest, getForestItems, type HelvegForestItem } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";
    import { get, writable } from "svelte/store";
    import TreeViewEntry from "./TreeViewEntry.svelte";

    const state = getContext<IExplorerState>("state");
    const graph = state.graph;
    const layoutOptions = state.layoutOptions;

    let additionalClass: string | undefined = undefined;
    export { additionalClass as class };

    export let style: string | undefined = undefined;

    let items: TreeViewItem[];
    const expanded = writable(new Set<string>());
    $: {
        items = getForestItems(
            getForest($graph, $layoutOptions.tidyTree.relation ?? "declares", state.diagram.options.nodeKindOrder),
        ) as TreeViewItem[];
        items.filter((i) => i.depth == 0).forEach((i) => get(expanded).add(i.id));
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

    export let selectedNode: string | null = null;

    function onNodeClicked(nodeId: string) {
        dispatch("nodeClicked", { nodeId: nodeId });
        selectedNode = nodeId;
    }

    function onNodeToggled(item: TreeViewItem, value?: boolean) {
        if (value === undefined) {
            value = !$expanded.has(item.id);
        }

        if (value) {
            $expanded = $expanded.add(item.id);
        } else {
            $expanded = (() => {
                $expanded.delete(item.id);
                return $expanded;
            })();
            for (const child of item.children ?? []) {
                if ($expanded.has(child.id)) {
                    onNodeToggled(child, false);
                }
            }
        }
    }
</script>

<div class="tree-view {additionalClass} flex flex-row" class:open={isOpen} {style}>
    <Panel id={AppPanels.TreeView} name="Tree View" class="flex flex-col flex-grow-1" indent={false}>
        {#if items.length > 0}
            {#each items as item (item.id)}
                <TreeViewEntry
                    isSelected={item.id === selectedNode}
                    isExpanded={$expanded.has(item.id)}
                    isParentExpanded={item.depth == 0 || (item.parent !== undefined && $expanded.has(item.parent?.id))}
                    node={item}
                    {onNodeClicked}
                    {onNodeToggled}
                />
            {/each}
        {:else}
            <p class="p-16">
                The displayed graph is empty or no relation is selected. Use the Layout panel to refresh the graph.
            </p>
        {/if}
    </Panel>
    <button class="toggle" on:click={togglePanel}>
        <Icon title={isOpen ? "Close" : "Open"} name={isOpen ? "vscode:chevron-left" : "vscode:chevron-right"} />
    </button>
</div>
