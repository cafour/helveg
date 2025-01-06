<script lang="ts">
    import { get, writable, type Writable } from "svelte/store";
    import type { TreeViewItem } from "./TreeView.svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_NODE_ICON } from "../deps/helveg-diagram";

    export let node: TreeViewItem;
    let isExpanded: Writable<boolean> = node.isExpanded;
    let isParentExpanded: Writable<boolean> = node.parent?.isExpanded ?? writable(true);
    export let isSelected: boolean;
    export let onNodeClicked: (nodeId: string) => void;

    function toggleItem(item: TreeViewItem, value?: boolean) {
        if (value === undefined) {
            value = !get(item.isExpanded);
        }

        item.isExpanded?.set(value);
        if (!value) {
            for (const child of item.children ?? []) {
                if (get(child.isExpanded)) {
                    toggleItem(child, false);
                }
            }
        }
    }
</script>

{#if node.depth == 0 || $isParentExpanded}
<div
    class="tree-view-node flex flex-col gap-2"
    class:item-hidden={node.parent && !$isParentExpanded}
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
                on:click={() => toggleItem(node)}
                class="flex flex-row align-items-center"
            >
                <Icon
                    title={$isExpanded ? "Collapse" : "Expand"}
                    name={$isExpanded
                        ? "vscode:chevron-down"
                        : "vscode:chevron-right"}
                />
            </button>
        {/if}
        <button
            class="flex flex-row gap-4 align-items-center flex-grow-1"
            on:click={() => onNodeClicked(node.id)}
        >
            <Icon
                title={node.node.kind}
                name={node.node.icon ?? FALLBACK_NODE_ICON}
            />
            <span>{node.node.label}</span>
        </button>
    </div>
</div>
{/if}
