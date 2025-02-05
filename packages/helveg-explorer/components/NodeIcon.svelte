<script lang="ts">
    import { getContext } from "svelte";
    import type { Diagram, MultigraphNode } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let node: string;
    $: attr = diagram.modelGraph.getNodeAttributes(node);

    const diagram = getContext<Diagram>("diagram");
    $: nodeStyle = diagram.nodeStylist(attr);
</script>

<Icon name={nodeStyle.icon} title={attr.model.kind} color={nodeStyle.color} />
