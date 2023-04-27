<script lang="ts">
    import type { GraphNode } from "model/multigraph";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";

    export let node: GraphNode | null = null;
    $: nodeItems =
        [
            ["Label", node?.label ?? null],
            ...Object.entries(node?.properties ?? {}),
        ].map((p) => ({
            key: p[0]!,
            value: p[1],
        })) ?? [];
</script>

<Panel name="Properties">
    {#if node == null}
        <span>Click on a node to view its properties.</span>
    {:else}
        <KeyValueList bind:items={nodeItems} />
    {/if}
</Panel>
