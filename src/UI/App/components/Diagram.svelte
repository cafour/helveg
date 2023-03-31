<script lang="ts">
    import { onMount } from "svelte";
    import type { Multigraph } from "../model/multigraph";
    import { Sigma } from "sigma";
    import Graph from "graphology";
    import circular from "graphology-layout/circular";
    import forceAtlas2 from "graphology-layout-forceatlas2";
    import FA2Layout from "graphology-layout-forceatlas2/worker";
    import type { VisualizationModel } from "model/visualization";
    import createNodePictogramProgram from "rendering/node.pictogram";
    import { getIcon, getIconDataUrl } from "model/icons";

    export let model: VisualizationModel;

    let element: HTMLElement;

    const graph = new Graph();
    for (const node of model.multigraph.nodes) {
        const entityKind = node.properties["EntityKind"];
        let size = 5;
        let color = "#202020";
        let pictogram = "base:PolarChart";
        switch (entityKind) {
            case "Solution":
                size = 9;
                color = "#ac162c"
                pictogram = "csharp:Solution";
                break;
            case "Project":
                size = 8;
                color = "#57a64a";
                pictogram = "csharp:CSProjectNode";
                break;
            case "Package":
            case "ExternalDependency":
                size = 7;
                color = "#002440";
                pictogram = "csharp:Package";
                break;
            case "Assembly":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Assembly";
                break;
            case "Module":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Module";
                break;
            case "Namespace":
                size = 5;
                color = "#dcdcdc";
                pictogram = "csharp:Namespace";
                break;
            case "Type":
                size = 4;
                color = "#4ec9b0";
                pictogram = "csharp:Class";
                break;
            case "TypeParameter":
                size = 2;
                color = "#4ec9b0";
                pictogram = "csharp:Type";
                break;
            case "Field":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Field";
                break;
            case "Method":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Method";
                break;
            case "Property":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Property";
                break;
            case "Event":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Event";
                break;
            case "Parameter":
                size = 1;
                color = "#9cdcfe";
                pictogram = "csharp:LocalVariable";
                break;
            default:
                break;
        }
        graph.addNode(node.id, {
            label: node.label || node.id,
            size: size,
            color: color,
            pictogramColor: color,
            type: "pictogram",
            pictogram: getIconDataUrl(pictogram)
        });
    }

    var containsRelation = model.multigraph.relations.filter((r) => r.id === "contains")[0];
    for (const edge of containsRelation.edges) {
        graph.addDirectedEdge(edge.src, edge.dst);
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
