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
    import type { StructuralState } from "model/structural";

    export let model: VisualizationModel;
    export let state: StructuralState;

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
            case "csharp:ExternalDependencySource":
                size = 8;
                color = "#002440";
                pictogram = "csharp:ReferenceGroup";
                break;
            case "csharp:Framework":
                size = 8;
                color = "#002440";
                pictogram = "csharp:Framework";
                break;
            case "csharp:Package":
                size = 7
                color = "#002440";
                pictogram = "csharp:Package";
                break;
            case "csharp:AssemblyDependency":
                size = 7;
                color = "#002440";
                pictogram = "csharp:Reference";
                break;
            case "csharp:AssemblyDefinition":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Assembly";
                break;
            case "csharp:ModuleDefinition":
                size = 6;
                color = "#57a64a";
                pictogram = "csharp:Module";
                break;
            case "csharp:NamespaceDefinition":
                size = 5;
                color = "#dcdcdc";
                pictogram = "csharp:Namespace";
                break;
            case "csharp:TypeDefinition":
                size = 4;
                color = "#4ec9b0";
                pictogram = "csharp:Class";
                break;
            case "csharp:TypeParameterDefinition":
                size = 2;
                color = "#4ec9b0";
                pictogram = "csharp:Type";
                break;
            case "csharp:FieldDefinition":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Field";
                break;
            case "csharp:MethodDefinition":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Method";
                break;
            case "csharp:PropertyDefinition":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Property";
                break;
            case "csharp:EventDefinition":
                size = 3;
                color = "#dcdcaa";
                pictogram = "csharp:Event";
                break;
            case "csharp:ParameterDefinition":
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
            pictogram: getIconDataUrl(pictogram, {
                width: 256,
                height: 256
            })
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

    let sigma: Sigma | null = null;
    
    onMount(() => {
        sigma = new Sigma(graph, element, {
            nodeProgramClasses: {
                pictogram: pictogramProgram
            },
            labelFont: "'Cascadia Mono', 'Consolas', monospace",
        });
        sigma.on("clickNode", e => {
            state.selectedNode = model.multigraph.nodes[e.node];
        });
    });
</script>

<div
    bind:this={element}
    class="diagram w-100p h-100p overflow-hidden absolute z-0"
/>
