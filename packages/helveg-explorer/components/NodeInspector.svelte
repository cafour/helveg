<script lang="ts">
    import { Diagram, type MultigraphNode } from "../deps/helveg-diagram.ts";
    import { getContext } from "svelte";
    import NodePreview from "./NodePreview.svelte";

    const diagram = getContext<Diagram>("diagram");

    export let node: MultigraphNode | null;
    $: inspection = node
        ? diagram.options.inspector(diagram.model.data!, node)
        : null;
</script>

<div class="node-inspector flex flex-col gap-4">
    <NodePreview {node} />
    <div class="expression flex flex-row flex-wrap text-sm">
        {#if inspection !== null}
            {#each inspection.expression.tokens as token}
                <code class="monospace {token.kind}" title={token.hint}
                    >{token.text.replaceAll(/\s+/g, "\xa0")}</code
                >
            {/each}
        {/if}
    </div>
</div>
