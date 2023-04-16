<script lang="ts">
    import { onMount } from "svelte";
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

    function addNode(nodeId: string) {
        const node = model.multigraph.nodes[nodeId];
        const entityKind = node.properties["Kind"];
        const glyphStyle = state.glyphOptions.styles[entityKind]
            ?? state.glyphOptions.fallbackStyle;
        const nodeStyle = glyphStyle.apply(node);
        graph.addNode(nodeId, {
            label: node.label || nodeId,
            size: nodeStyle.size,
            color: nodeStyle.color,
            pictogramColor: nodeStyle.color,
            type: "pictogram",
            pictogram: getIconDataUrl(nodeStyle.icon, {
                width: 256,
                height: 256
            })
        });
    }

    for (const nodeId in model.multigraph.nodes) {
        addNode(nodeId);
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

    function collapseNode(nodeId: string) {
        graph.forEachOutNeighbor(nodeId, neighborId => {
            collapseNode(neighborId);
            graph.dropNode(neighborId);
        });
        graph.setNodeAttribute(nodeId, "collapsed", true);
    }
    
    function expandNode(nodeId: string, recursive: boolean = false) {
        let x = graph.getNodeAttribute(nodeId, "x");
        let y = graph.getNodeAttribute(nodeId, "y");
        let neighbours = [...declaresRelation.edges
            .filter(edge => edge.src === nodeId)];
        neighbours.forEach((edge, i) => {
                addNode(edge.dst);
                graph.setNodeAttribute(edge.dst, "x", x + Math.cos(i / neighbours.length * 2 * Math.PI));
                graph.setNodeAttribute(edge.dst, "y", y + Math.sin(i / neighbours.length * 2 * Math.PI));
                graph.addDirectedEdge(edge.src, edge.dst);
                if (recursive) {
                    expandNode(edge.dst, true);
                }
            });

        graph.setNodeAttribute(nodeId, "collapsed", false);
    }

    function toggleNode(nodeId: string) {
        const collapsed = <boolean | undefined>(graph.getNodeAttribute(nodeId, "collapsed"));
        if (collapsed) {
            expandNode(nodeId);
        }
        else {
            collapseNode(nodeId);
        }
    }

    onMount(() => {
        sigma = new Sigma(graph, element, {
            nodeProgramClasses: {
                pictogram: pictogramProgram
            },
            labelFont: "'Cascadia Mono', 'Consolas', monospace"
        });
        sigma.on("clickNode", e => {
            state.selectedNode = model.multigraph.nodes[e.node];
        });
        sigma.on("doubleClickNode", e => {
            toggleNode(e.node);
        });
    });
</script>

<div
    bind:this={element}
    class="diagram w-100p h-100p overflow-hidden absolute z-0"
/>
