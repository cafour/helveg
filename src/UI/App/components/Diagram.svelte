<script lang="ts">
    import { onMount } from "svelte";
    import type { Multigraph } from "../model/multigraph";
    import { Sigma } from "sigma";
    import Graph from "graphology";
    import circular from "graphology-layout/circular";
    import forceAtlas2 from "graphology-layout-forceatlas2";
    import FA2Layout from "graphology-layout-forceatlas2/worker";
    import type { VisualizationModel } from "model/visualization";

    export let model: VisualizationModel;

    let element: HTMLElement;

    const graph = new Graph();
    for (const node of model.multigraph.nodes) {
        const entityKind = node.properties["EntityKind"];
        let size = 5;
        let color = "#202020";
        switch (entityKind) {
            case "Solution":
                size = 9;
                color = "#ac162c"
                break;
            case "Project":
                size = 8;
                color = "#57a64a";
                break;
            case "Package":
            case "ExternalDependency":
                size = 7;
                color = "#002440";
                break;
            case "Assembly":
                size = 6;
                color = "#57a64a";
                break;
            case "Module":
                size = 6;
                color = "#57a64a";
                break;
            case "Namespace":
                size = 5;
                color = "#dcdcdc";
                break;
            case "Type":
                size = 4;
                color = "#4ec9b0";
                break;
            case "TypeParameter":
                size = 2;
                color = "#4ec9b0";
                break;
            case "Field":
            case "Method":
            case "Property":
            case "Event":
                size = 3;
                color = "#dcdcaa";
                break;
            case "Parameter":
                size = 1;
                color = "#9cdcfe";
                break;
            default:
                break;
        }
        graph.addNode(node.id, {
            label: node.label || node.id,
            size: size,
            color: color
        });
        // graph.setNodeAttribute(node.id, "size", 10);
    }

    var containsRelation = model.multigraph.relations.filter((r) => r.id === "contains")[0];
    for (const edge of containsRelation.edges) {
        graph.addDirectedEdge(edge.src, edge.dst);
    }

    // circular.assign(graph);
    let settings = forceAtlas2.inferSettings(graph);
    settings.adjustSizes = true;
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
