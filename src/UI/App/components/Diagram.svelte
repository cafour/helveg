<script lang="ts">
  import { onMount } from "svelte";
  import type { Multigraph } from "../model/data";
  import { Sigma } from "sigma";
  import Graph from "graphology";
  import circular from "graphology-layout/circular";
  import forceAtlas2 from "graphology-layout-forceatlas2";

  export let dataId = "helveg-data";

  let element: HTMLElement;
  const dataScript = document.getElementById(dataId);
  if (dataScript == null) {
    throw new Error(`Could not find the '${dataId}' element.`);
  }

  const data = <Multigraph>JSON.parse(dataScript.textContent!);
  const graph = new Graph();
  for (const node of data.Nodes) {
    graph.addNode(node.Id, {
        label: node.Label || node.Id
    });
    graph.setNodeAttribute(node.Id, "size", 10);
  }

  var containsRelation = data.Relations.filter(r => r.Id === "contains")[0];
  for (const edge of containsRelation.Edges) {
    graph.addDirectedEdge(edge.Src, edge.Dst);
  }

  circular.assign(graph);
  const settings = forceAtlas2.inferSettings(graph);
  forceAtlas2.assign(graph, { settings, iterations: 600 });

  onMount(() => {
    new Sigma(graph, element);
  });
</script>

<div bind:this={element} class=sigma-container />
