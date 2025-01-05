<script lang="ts">
    import { createEventDispatcher, getContext, onMount } from "svelte";
    import { AppPanels } from "../const";
    import Panel from "./Panel.svelte";
    import type { IExplorerState } from "../explorer-state";
    import { getForest } from "../deps/helveg-diagram.ts";
    import TreeViewNode from "./TreeViewNode.svelte";
    import Icon from "./Icon.svelte";

    const state = getContext<IExplorerState>("state");
    const graph = state.graph;
    const layoutOptions = state.layoutOptions;

    let additionalClass: string | undefined = undefined;
    export { additionalClass as class };

    $: forest = getForest(
        $graph,
        $layoutOptions.tidyTree.relation ?? "declares",
    );

    let isOpen = false;

    onMount(() => {
        isOpen = localStorage.getItem(`TreeView.isOpen`) === "true";
    });

    export function toggle() {
        isOpen = !isOpen;
        localStorage.setItem(`TreeView.isOpen`, isOpen ? "true" : "false");
    }

    let dispatch = createEventDispatcher();
</script>

<div class="tree-view {additionalClass} flex flex-row" class:open={isOpen}>
    <Panel id={AppPanels.TreeView} name="Tree View" class="flex-grow-1">
        {#if forest.roots.length > 0}
            {#each forest.roots as root}
                <TreeViewNode
                    node={root}
                    onNodeClicked={(nodeId) =>
                        dispatch("nodeClicked", { nodeId: nodeId })}
                />
            {/each}
        {:else}
            <i
                >The displayed graph is empty or no relation is selected. Use
                the Layout panel to refresh the graph.</i
            >
        {/if}
    </Panel>
    <button class="toggle" on:click={toggle}>
        <Icon
            title={isOpen ? "Close" : "Open"}
            name={isOpen ? "vscode:chevron-left" : "vscode:chevron-right"}
        />
    </button>
</div>
