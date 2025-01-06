<script lang="ts">
    import { Diagram, type MultigraphNode } from "../deps/helveg-diagram.ts";
    import { getContext } from "svelte";

    const diagram = getContext<Diagram>("diagram");

    export let node: MultigraphNode;
    $: inspection = diagram.options.inspector(diagram.model.data!, node);
</script>

<div class="node-inspector flex flex-col gap-4">
    <div class="expression flex flex-row flex-wrap">
        {#each inspection.expression.tokens as token}
            <code class="monospace {token.kind}">{token.text.replaceAll(/\s+/g, '\xa0')}</code>
        {/each}
    </div>
</div>
