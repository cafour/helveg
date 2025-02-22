<script lang="ts">
    import { getContext, onDestroy } from "svelte";
    import type { Diagram, MultigraphNode } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let node: string;
    $: attr = diagram.modelGraph.getNodeAttributes(node);

    const diagram = getContext<Diagram>("diagram");
    $: nodeStyle = diagram.nodeStylist(attr, diagram.nodeStylistParams);

    function onNodeStylistParamsChanged() {
        nodeStyle = diagram.nodeStylist(attr, diagram.nodeStylistParams);
    }

    diagram.events.nodeStylistParamsChanged.subscribe(onNodeStylistParamsChanged);

    onDestroy(() => diagram?.events?.nodeStylistParamsChanged?.unsubscribe(onNodeStylistParamsChanged));
</script>

<Icon name={nodeStyle.icon} title={attr.model.kind} color={nodeStyle.color} />
