<script lang="ts">
    import { onMount } from "svelte";
    import type { Multigraph } from "../model/multigraph";
    import { Sigma } from "sigma";
    import Graph, { NotFoundGraphError, UsageGraphError } from "graphology";
    import circular from "graphology-layout/circular";
    import forceAtlas2 from "graphology-layout-forceatlas2";
    import FA2Layout from "graphology-layout-forceatlas2/worker";
    import type { VisualizationModel } from "model/visualization";
    import createNodePictogramProgram from "rendering/node.pictogram";
    import { getIconDataUrl } from "model/icons";

    export let model: VisualizationModel;

    let element: HTMLElement;

    const graph = new Graph();
    for (const nodeId in model.multigraph.nodes) {
        const node = model.multigraph.nodes[nodeId];
        const entityKind = node.properties["Kind"];
        let size = 5;
        let color = "#202020";
        let pictogram = "base:PolarChart";
        switch (entityKind) {
            case "csharp:Solution":
                size = 9;
                color = "#ac162c"
                pictogram = "csharp:Solution";
                break;
            case "csharp:Project":
                size = 8;
                color = "#57a64a";
                pictogram = "csharp:CSProjectNode";
                break;
            case "csharp:Package":
            case "csharp:ExternalDependency":
                size = 7;
                color = "#002440";
                pictogram = "csharp:Package";
                break;
            case "csharp:Assembly":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Assembly";
                break;
            case "csharp:Module":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Module";
                break;
            case "csharp:Namespace":
                size = 5;
                color = "#dcdcdc";
                pictogram = "csharp:Namespace";
                break;
            case "csharp:Type":
                size = 4;
                color = "#4ec9b0";
                pictogram = "csharp:Class";
                break;
            case "csharp:TypeParameter":
                size = 2;
                color = "#4ec9b0";
                pictogram = "csharp:Type";
                break;
            case "csharp:Field":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Field";
                break;
            case "csharp:Method":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Method";
                break;
            case "csharp:Property":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Property";
                break;
            case "csharp:Event":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Event";
                break;
            case "csharp:Parameter":
                size = 1;
                color = "#9cdcfe";
                pictogram = "csharp:LocalVariable";
                break;
            default:
                break;
        }
        graph.addNode(nodeId, {
            label: node.label || nodeId,
            size: size,
            color: color,
            pictogramColor: color,
            type: "pictogram",
            pictogram: getIconDataUrl(pictogram)
        });
    }

    var declaresRelation = model.multigraph.relations["declares"];
    for (const edge of declaresRelation.edges) {
        try {
            graph.addDirectedEdge(edge.src, edge.dst);
        }
        catch (error)
        {
            console.warn("Failed to add an edge. edge={}, error={}", edge, error);
        }
    }

    circular.assign(graph);
    let settings = forceAtlas2.inferSettings(graph);
    settings.adjustSizes = true;
    const worker = new FA2Layout(graph, {
        settings: settings
    });
    worker.start();

    const pictogramProgram = createNodePictogramProgram({
        // correctCentering: true,
        // forcedSvgSize: 64,
        // keepWithinCircle: true
    });

    onMount(() => {
        new Sigma(graph, element, {
            nodeProgramClasses: {
                pictogram: pictogramProgram
            },
            labelFont: "'Cascadia Mono', 'Consolas', monospace"
        });
    });
</script>

<div
    bind:this={element}
    class="diagram w-100p h-100p overflow-hidden absolute z-0"
/>
