<script lang="ts">
    import { onMount } from "svelte";
    import { Sigma } from "sigma";
    import Graph from "graphology";
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

    let graph: Graph | null = null
    let sigma: Sigma | null = null;
    let worker: FA2Layout | null = null;

    function addNode(graph: Graph, nodeId: string) {
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

    function initializeGraph(model: VisualizationModel) {
        const graph = new Graph();
        for (const nodeId in model.multigraph.nodes) {
            addNode(graph, nodeId);
        }
        
        var declaresRelation = model.multigraph.relations["declares"];
        if (!declaresRelation) {
            console.warn("The visualization model does not contain the required 'declares' relation.");
            return graph;
        }

        for (const edge of declaresRelation.edges) {
            try {
                graph.addDirectedEdge(edge.src, edge.dst);
            }
            catch (error)
            {
                console.warn(`Failed to add an edge. edge=${edge}, error=${error}`);
            }
        }

        circular.assign(graph);
        return graph;
    }

    const pictogramProgram = createNodePictogramProgram();

    function collapseNode(graph: Graph, nodeId: string) {
        graph.forEachOutNeighbor(nodeId, neighborId => {
            collapseNode(graph, neighborId);
            graph.dropNode(neighborId);
        });
        graph.setNodeAttribute(nodeId, "collapsed", true);
    }

    function expandNode(graph: Graph, nodeId: string, recursive: boolean = false) {
        let declaresRelation = helveg.model.multigraph.relations["declares"];
        let x = graph.getNodeAttribute(nodeId, "x");
        let y = graph.getNodeAttribute(nodeId, "y");
        let neighbours = [...declaresRelation.edges
            .filter(edge => edge.src === nodeId)];
        neighbours.forEach((edge, i) => {
                addNode(graph, edge.dst);
                graph.setNodeAttribute(edge.dst, "x", x + Math.cos(i / neighbours.length * 2 * Math.PI));
                graph.setNodeAttribute(edge.dst, "y", y + Math.sin(i / neighbours.length * 2 * Math.PI));
                graph.addDirectedEdge(edge.src, edge.dst);
                if (recursive) {
                    expandNode(graph, edge.dst, true);
                }
            });

        graph.setNodeAttribute(nodeId, "collapsed", false);
    }

    function toggleNode(graph: Graph, nodeId: string) {
        const collapsed = <boolean | undefined>(graph.getNodeAttribute(nodeId, "collapsed"));
        if (collapsed) {
            expandNode(graph, nodeId);
        }
        else {
            collapseNode(graph, nodeId);
        }
    }
    
    function initializeSigma(graph: Graph) {
        let sigma = new Sigma(graph, element, {
            nodeProgramClasses: {
                pictogram: pictogramProgram
            },
            labelFont: "'Cascadia Mono', 'Consolas', monospace"
        });
        sigma.on("clickNode", e => {
            state.selectedNode = model.multigraph.nodes[e.node];
        });
        sigma.on("doubleClickNode", e => {
            toggleNode(graph, e.node);
        });
        return sigma;
    }
    
    function initializeWorker(graph: Graph) {
        let settings = forceAtlas2.inferSettings(graph);
        settings.adjustSizes = true;
        const worker = new FA2Layout(graph, {
            settings: settings
        });
        worker.start();
        return worker;
    }

    $: if (element) {
        graph = initializeGraph(model);
        worker = initializeWorker(graph);
        sigma = initializeSigma(graph);
    }
    
    export function run() {
        if (!worker?.isRunning()) {
            worker?.start();
        }
    }

    export function stop() {
        worker?.stop();
    }
</script>

<div
    bind:this={element}
    class="diagram w-100p h-100p overflow-hidden absolute z-0"
/>
