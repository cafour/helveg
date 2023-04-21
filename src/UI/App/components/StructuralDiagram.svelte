<script lang="ts">
    import { Sigma } from "sigma";
    import Graph from "graphology";
    import circular from "graphology-layout/circular";
    import forceAtlas2 from "graphology-layout-forceatlas2";
    import {
        ForceAtlas2Supervisor,
        type ForceAtlas2Progress,
    } from "layout/forceAltas2Supervisor";
    import type { VisualizationModel } from "model/visualization";
    import { StructuralStatus, type StructuralState } from "model/structural";
    import Icon from "./Icon.svelte";
    import tidyTree from "layout/tidyTree";
    import { IconAtlas } from "rendering/iconAtlas";
    import createIconProgram from "rendering/node.icon";
    import createOutlinesProgram from "rendering/node.outlines";
    import { createNodeCompoundProgram } from "sigma/rendering/webgl/programs/common/node";
    import { OutlineStyle, getOutlinesWidth, type Outlines } from "model/glyph";

    export let model: VisualizationModel;
    export let state: StructuralState;
    export let iterations: number = 0;
    export let speed: number = 0;

    let diagramElement: HTMLElement;
    let loadingScreenElement: HTMLElement;

    let graph: Graph | null = null;
    let sigma: Sigma | null = null;
    let supervisor: ForceAtlas2Supervisor | null = null;

    let iconAtlas = new IconAtlas();
    let iconProgram = createIconProgram(iconAtlas, "#00000000");
    let outlinesProgram = createOutlinesProgram(0.5);
    let glyphProgram = createNodeCompoundProgram([
        outlinesProgram,
        iconProgram,
    ]);

    function addNode(graph: Graph, nodeId: string) {
        const node = model.multigraph.nodes[nodeId];
        const entityKind = node.properties["Kind"];
        const glyphStyle =
            state.glyphOptions.styles[entityKind] ??
            state.glyphOptions.fallbackStyle;
        const nodeStyle = glyphStyle.apply(node);
        const outlines = [
            { width: nodeStyle.size, style: OutlineStyle.Solid },
            ...nodeStyle.outlines.slice(0, 3),
        ] as Outlines;
        graph.addNode(nodeId, {
            label: node.label || nodeId,
            size: outlines.length > 0 ? getOutlinesWidth(outlines) : nodeStyle.size,
            iconSize: nodeStyle.size,
            color: nodeStyle.color,
            type: "glyph",
            icon: nodeStyle.icon,
            outlines: outlines,
        });
    }

    function initializeGraph(model: VisualizationModel) {
        if (supervisor != null) {
            supervisor.kill();
        }

        const graph = new Graph();
        for (const nodeId in model.multigraph.nodes) {
            addNode(graph, nodeId);
        }

        var declaresRelation = model.multigraph.relations["declares"];
        if (!declaresRelation) {
            console.warn(
                "The visualization model does not contain the required 'declares' relation."
            );
            return graph;
        }

        for (const edge of declaresRelation.edges) {
            try {
                graph.addDirectedEdge(edge.src, edge.dst);
            } catch (error) {
                // console.warn(`Failed to add an edge. edge=${edge}, error=${error}`);
            }
        }

        circular.assign(graph);
        let solutionRoot = Object.entries(model.multigraph.nodes).find(
            ([k, v]) => v.properties["Kind"] === "csharp:Solution"
        )?.[0];
        let frameworkRoots = Object.entries(model.multigraph.nodes).find(
            ([k, v]) => v.properties["Kind"] === "csharp:Framework"
        )?.[0];
        if (solutionRoot) {
            tidyTree(graph, solutionRoot, 1000);
        }
        return graph;
    }

    function collapseNode(graph: Graph, nodeId: string) {
        graph.forEachOutNeighbor(nodeId, (neighborId) => {
            collapseNode(graph, neighborId);
            graph.dropNode(neighborId);
        });
        graph.setNodeAttribute(nodeId, "collapsed", true);
    }

    function expandNode(
        graph: Graph,
        nodeId: string,
        recursive: boolean = false
    ) {
        let declaresRelation = helveg.model.multigraph.relations["declares"];
        let x = graph.getNodeAttribute(nodeId, "x");
        let y = graph.getNodeAttribute(nodeId, "y");
        let neighbours = [
            ...declaresRelation.edges.filter((edge) => edge.src === nodeId),
        ];
        neighbours.forEach((edge, i) => {
            addNode(graph, edge.dst);
            graph.setNodeAttribute(
                edge.dst,
                "x",
                x + Math.cos((i / neighbours.length) * 2 * Math.PI)
            );
            graph.setNodeAttribute(
                edge.dst,
                "y",
                y + Math.sin((i / neighbours.length) * 2 * Math.PI)
            );
            graph.addDirectedEdge(edge.src, edge.dst);
            if (recursive) {
                expandNode(graph, edge.dst, true);
            }
        });

        graph.setNodeAttribute(nodeId, "collapsed", false);
    }

    function toggleNode(graph: Graph, nodeId: string) {
        const collapsed = <boolean | undefined>(
            graph.getNodeAttribute(nodeId, "collapsed")
        );
        if (collapsed) {
            expandNode(graph, nodeId);
        } else {
            collapseNode(graph, nodeId);
        }
    }

    function initializeSigma(graph: Graph) {
        let sigma = new Sigma(graph, diagramElement, {
            nodeProgramClasses: {
                glyph: glyphProgram,
            },
            labelFont: "'Cascadia Mono', 'Consolas', monospace",
            itemSizesReference: "positions",
        });
        sigma.on("clickNode", (e) => {
            state.selectedNode = model.multigraph.nodes[e.node];
        });
        // sigma.on("doubleClickNode", (e) => {
        //     toggleNode(graph, e.node);
        // });
        return sigma;
    }

    function onSupervisorProgress(message: ForceAtlas2Progress) {
        iterations = message.iterationCount;
        speed = message.speed;
    }

    function initializeSupervisor(
        existingSupervisor: ForceAtlas2Supervisor | null,
        graph: Graph
    ) {
        if (existingSupervisor != null) {
            existingSupervisor.progress.unsubscribe(onSupervisorProgress);
            existingSupervisor.kill();
        }

        let settings = forceAtlas2.inferSettings(graph);
        settings.adjustSizes = true;
        supervisor = new ForceAtlas2Supervisor(graph, settings);
        supervisor.progress.subscribe(onSupervisorProgress);
        return supervisor;
    }

    $: if (diagramElement) {
        graph = initializeGraph(model);
        sigma = initializeSigma(graph);
        supervisor = initializeSupervisor(supervisor, graph);
        if (!model.isEmpty) {
            run();
        }
    }

    export function tidy() {}

    export async function run(inBackground: boolean = false) {
        if (graph == null) {
            console.warn("Cannot run since the graph is not initialized.");
            return;
        }

        if (supervisor == null) {
            console.warn("Cannot run since the supervisor is not initialized.");
            return;
        }

        supervisor.start(inBackground);

        state.status = inBackground
            ? StructuralStatus.RunningInBackground
            : StructuralStatus.Running;

        if (inBackground) {
            loadingScreenElement.classList.remove("hidden");
            sigma?.kill();
            sigma = null;
        } else {
            loadingScreenElement.classList.add("hidden");
            if (!sigma) {
                sigma = initializeSigma(graph);
            }
        }
    }

    export async function stop() {
        if (graph == null) {
            console.warn("Cannot stop since the graph is not initialized.");
            return;
        }

        if (supervisor?.isRunning) {
            await supervisor.stop();
            state.status = StructuralStatus.Stopped;
            loadingScreenElement.classList.add("hidden");

            if (!sigma) {
                sigma = initializeSigma(graph);
            }
        }
    }
</script>

<div
    bind:this={loadingScreenElement}
    class="loading-screen w-100p overflow-hidden h-100p absolute z-1 flex flex-col align-items-center justify-content-center bg-surface-50 hidden"
>
    <div class="w-32 h-32">
        <Icon name="Fallback" />
    </div>
    <div>Running in the background...</div>
</div>

<div
    bind:this={diagramElement}
    class="diagram w-100p h-100p overflow-hidden absolute z-0"
/>
