<script lang="ts">
    import { getContext } from "svelte";
    import type { Diagram, MultigraphNode } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let node: MultigraphNode;

    const diagram = getContext<Diagram>("diagram");
    $: nodeStyle = diagram.nodeStylist(node);
</script>

<Icon name={nodeStyle.icon} title={node.kind} color={nodeStyle.color} />
