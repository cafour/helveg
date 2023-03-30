<script lang="ts">
    import { onMount } from "svelte";
    import type { Multigraph } from "../model/data";
    import { Sigma } from "sigma";
    import Graph from "graphology";
    import circular from "graphology-layout/circular";
    import forceAtlas2 from "graphology-layout-forceatlas2";
    import FA2Layout from "graphology-layout-forceatlas2/worker";

    export let dataId = "helveg-multigraph";

    let element: HTMLElement;
    const dataScript = document.getElementById(dataId);
    if (dataScript == null) {
        throw new Error(`Could not find the '${dataId}' element.`);
    }

    const data = <Multigraph>JSON.parse(dataScript.textContent!);
    const graph = new Graph();
    for (const node of data.nodes) {
        graph.addNode(node.id, {
            label: node.label || node.id,
        });
        graph.setNodeAttribute(node.id, "size", 10);
    }

    var containsRelation = data.relations.filter((r) => r.id === "contains")[0];
    for (const edge of containsRelation.edges) {
        graph.addDirectedEdge(edge.src, edge.dst);
    }

    circular.assign(graph);
    const settings = forceAtlas2.inferSettings(graph);
    const worker = new FA2Layout(graph, {
        settings: settings
    });
    worker.start();

    onMount(() => {
        new Sigma(graph, element, {
            
        });
    });
</script>

<div bind:this={element} class="diagram w-100p h-100p overflow-hidden absolute z-0" />
