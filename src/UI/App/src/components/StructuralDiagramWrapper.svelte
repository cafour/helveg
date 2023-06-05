<script lang="ts">
    import {
        StructuralStatus,
        StructuralDiagram,
        type AbstractStructuralDiagram,
        type StructuralDiagramStats,
    } from "model/structural";
    import Icon from "./Icon.svelte";
    import type {
    DataOptions,
        ExportOptions,
        GlyphOptions,
        LayoutOptions,
        SearchMode,
        ToolOptions,
    } from "model/options";
    import { createEventDispatcher, getContext } from "svelte";
    import type { HelvegInstance } from "model/instance";
    import type { Readable, Writable } from "svelte/store";
    import type { VisualizationModel } from "model/visualization";

    let instance = getContext<HelvegInstance>("helveg");
    let model = getContext<Readable<VisualizationModel>>("model");
    let layoutOptions = getContext<Writable<LayoutOptions>>("layoutOptions");
    let toolOptions = getContext<Writable<ToolOptions>>("toolOptions");
    let dataOptions = getContext<Writable<DataOptions>>("dataOptions");
    let glyphOptions = getContext<Writable<GlyphOptions>>("glyphOptions");

    let diagram: AbstractStructuralDiagram = new StructuralDiagram(instance);
    
    export function getDiagram(): AbstractStructuralDiagram {
        return diagram;
    }

    export let status: StructuralStatus = StructuralStatus.Stopped;
    export let stats: StructuralDiagramStats = {
        iterationCount: 0,
        speed: 0,
    };
    export let selectedNodeId: string | null = null;
    export let canDragNodes: boolean = false;

    diagram.statusChanged.subscribe((s) => {
        status = s;
    });
    diagram.statsChanged.subscribe((s) => {
        DEBUG && console.log("Stats changed", s);
        stats = s;
    });
    diagram.nodeSelected.subscribe((n) => {
        selectedNodeId = n;
    });
    diagram.nodeClicked.subscribe((n) => {
        dispatch("nodeClicked", n);
    });

    let diagramElement: HTMLElement | null = null;
    let loadingScreenElement: HTMLElement;

    $: diagram.element = diagramElement;
    $: diagram.model = $model;
    $: if (!$model.isEmpty) {
        resetLayout();
    }

    $: diagram.dataOptions = $dataOptions;
    $: diagram.layoutOptions = $layoutOptions;
    $: diagram.glyphOptions = $glyphOptions;
    $: diagram.toolOptions = $toolOptions;
    $: diagram.canDragNodes = canDragNodes;

    export function resetLayout() {
        return diagram.resetLayout();
    }

    export function runLayout(inBackground: boolean) {
        return diagram.runLayout(inBackground);
    }

    export function stopLayout() {
        return diagram.stopLayout();
    }

    export function save(options?: ExportOptions) {
        diagram.save(options);
    }

    export function highlight(
        searchText: string | null,
        searchMode: SearchMode
    ) {
        diagram.highlight(searchText, searchMode);
    }

    export function isolate(searchText: string | null, searchMode: SearchMode) {
        return diagram.isolate(searchText, searchMode);
    }

    export function refresh() {
        return diagram.refresh();
    }

    export function cut(nodeId: string) {
        return diagram.cut(nodeId);
    }
    
    export function toggleNode(nodeId: string) {
        return diagram.toggleNode(nodeId);
    }
    
    export function highlightNode(nodeId: string | null, includeSubtree: boolean, includeNeighbors: boolean) {
        diagram.highlightNode(nodeId, includeSubtree, includeNeighbors);
    }

    let dispatch = createEventDispatcher();

    diagram.nodeSelected.subscribe((n) => {
        dispatch("nodeSelected", n);
    });
</script>

<div
    bind:this={loadingScreenElement}
    class="loading-screen flex flex-col"
    class:hidden={status !== StructuralStatus.RunningInBackground}
>
    <div class="w-32 h-32">
        <Icon name="Fallback" />
    </div>
    <div>Running in the background...</div>
</div>

<div bind:this={diagramElement} class="diagram" />
