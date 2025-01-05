<script lang="ts">
    import {
        FALLBACK_NODE_ICON,
        type HelvegTree,
    } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let node: HelvegTree;
    export let isExpanded = false;
    export let onNodeClicked: (nodeId: string) => void;
</script>

<div class="tree-view-node flex flex-col gap-2">
    <div class="flex flex-row gap-4 align-items-center">
        {#if node.children === undefined || node.children.length == 0}
            <Icon title="" name="vscode:blank" />
        {:else}
            <button on:click={() => (isExpanded = !isExpanded)} class="flex flex-row align-items-center">
                <Icon
                    title={isExpanded ? "Collapse" : "Expand"}
                    name={isExpanded
                        ? "vscode:chevron-down"
                        : "vscode:chevron-right"}
                />
            </button>
        {/if}
        <button class="flex flex-row gap-4 align-items-center" on:click={() => onNodeClicked(node.id)}>
            <Icon
                title={node.node.kind}
                name={node.node.icon ?? FALLBACK_NODE_ICON}
            />
            <span>{node.node.label}</span>
        </button>
    </div>
    {#if node.children !== undefined && node.children.length > 0 && isExpanded}
        <div class="pl-8">
            {#each node.children as child}
                <svelte:self node={child} onNodeClicked={onNodeClicked} />
            {/each}
        </div>
    {/if}
</div>
