<script lang="ts">
    import type { TreeViewItem } from "./TreeView.svelte";
    import Icon from "./Icon.svelte";
    import NodeIcon from "./NodeIcon.svelte";

    export let node: TreeViewItem;
    export let isExpanded: boolean;
    export let isParentExpanded: boolean;
    export let isSelected: boolean;
    export let onNodeClicked: (nodeId: string) => void;
    export let onNodeToggled: (item: TreeViewItem) => void;
</script>

{#if node.depth == 0 || isParentExpanded}
<div
    class="tree-view-node flex flex-col gap-2"
    class:item-hidden={!isParentExpanded}
    class:selected={isSelected}
>
    <div
        class="flex flex-row gap-4 align-items-center"
        style="padding-left: {0.5 + node.depth * 0.5}rem;"
    >
        {#if node.children === undefined || node.children?.length == 0}
            <Icon title="" name="vscode:blank" />
        {:else}
            <button
                on:click={() => onNodeToggled(node)}
                class="flex flex-row align-items-center"
            >
                <Icon
                    title={isExpanded ? "Collapse" : "Expand"}
                    name={isExpanded
                        ? "vscode:chevron-down"
                        : "vscode:chevron-right"}
                />
            </button>
        {/if}
        <button
            class="flex flex-row gap-4 align-items-center flex-grow-1"
            on:click={() => onNodeClicked(node.id)}
        >
            <NodeIcon node={node.id} />
            <span>{node.node.label}</span>
        </button>
    </div>
</div>
{/if}
