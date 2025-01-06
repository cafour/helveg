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
    import TreeViewNode from "./TreeViewNode.svelte";
    import Icon from "./Icon.svelte";
    import VirtualList from "svelte-tiny-virtual-list";

    const state = getContext<IExplorerState>("state");
    const graph = state.graph;
    const layoutOptions = state.layoutOptions;

    let additionalClass: string | undefined = undefined;
    export { additionalClass as class };

    type TreeViewItem = HelvegForestItem & {
        isExpanded?: boolean;
        parent?: TreeViewItem;
        children?: TreeViewItem[];
    };

    $: items = getForestItems(
        getForest($graph, $layoutOptions.tidyTree.relation ?? "declares"),
    ) as TreeViewItem[];

    let isOpen = false;

    onMount(() => {
        isOpen = localStorage.getItem(`TreeView.isOpen`) === "true";
    });

    export function togglePanel() {
        isOpen = !isOpen;
        localStorage.setItem(`TreeView.isOpen`, isOpen ? "true" : "false");
    }

    let dispatch = createEventDispatcher();

    function onNodeClicked(nodeId: string | undefined) {
        dispatch("nodeClicked", { nodeId: nodeId });
    }

    export function selectNode(nodeId: string | undefined) {}

    function getItemHeight(index: number) {
        const item = items[index];
        // 24 = 1.5rem which _should_ be the item height
        return item.depth == 0 ||
            item.parent === undefined ||
            item.parent.isExpanded
            ? 24
            : 1;
    }

    let virtualList: VirtualList;

    function toggleItem(index: number) {
        items[index].isExpanded = !items[index].isExpanded;
        virtualList.recomputeSizes(index + 1);
    }
</script>

<div class="tree-view {additionalClass} flex flex-row" class:open={isOpen}>
    <Panel id={AppPanels.TreeView} name="Tree View" class="flex-grow-1">
        {#if items.length > 0}
            <VirtualList
                bind:this={virtualList}
                height="100%"
                itemCount={items.length}
                itemSize={getItemHeight}
            >
                <div
                    slot="item"
                    class="tree-view-node flex flex-col gap-2"
                    let:index
                    let:style
                    {style}
                    class:item-hidden={items[index].parent &&
                        !items[index].parent?.isExpanded}
                >
                    {#if items[index] !== undefined}
                        <div
                            class="flex flex-row gap-4 align-items-center"
                            style="padding-left: {items[index].depth * 0.5}rem;"
                        >
                            {#if items[index].children === undefined || items[index].children?.length == 0}
                                <Icon title="" name="vscode:blank" />
                            {:else}
                                <button
                                    on:click={() => toggleItem(index)}
                                    class="flex flex-row align-items-center"
                                >
                                    <Icon
                                        title={items[index].isExpanded
                                            ? "Collapse"
                                            : "Expand"}
                                        name={items[index].isExpanded
                                            ? "vscode:chevron-down"
                                            : "vscode:chevron-right"}
                                    />
                                </button>
                            {/if}
                            <button
                                class="flex flex-row gap-4 align-items-center"
                                on:click={() => onNodeClicked(items[index].id)}
                            >
                                <Icon
                                    title={items[index].node.kind}
                                    name={items[index].node.icon ??
                                        FALLBACK_NODE_ICON}
                                />
                                <span>{items[index].node.label}</span>
                            </button>
                        </div>
                    {/if}
                </div>
            </VirtualList>
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
