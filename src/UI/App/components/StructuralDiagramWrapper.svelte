<script lang="ts">
    import type { VisualizationModel } from "model/visualization";
    import { StructuralStatus, StructuralDiagram, type AbstractStructuralDiagram, type StructuralDiagramStats } from "model/structural";
    import Icon from "./Icon.svelte";
    import type { DataOptions, ExportOptions, GlyphOptions, HelvegOptions, LayoutOptions, SearchMode } from "model/options";
    import { getContext } from "svelte";
    import type { HelvegInstance } from "model/instance";

    export let model: VisualizationModel;
    export let dataOptions: DataOptions;
    export let layoutOptions: LayoutOptions;
    export let glyphOptions: GlyphOptions;

    let instance = getContext<HelvegInstance>("helveg");
    let diagram: AbstractStructuralDiagram = new StructuralDiagram(instance);

    export let status: StructuralStatus = StructuralStatus.Stopped;
    export let stats: StructuralDiagramStats = {
        iterationCount: 0,
        speed: 0
    };
    export let selectedNodeId: string | null = null;

    diagram.statusChanged.subscribe(s => {
        status = s;
    });
    diagram.statsChanged.subscribe(s => {
        DEBUG && console.log("Stats changed", s);
        stats = s;
    });
    diagram.nodeSelected.subscribe(n => {
        selectedNodeId = n;
    });

    let diagramElement: HTMLElement | null = null;
    let loadingScreenElement: HTMLElement;

    $: diagram.element = diagramElement;
    $: diagram.model = model;
    $: if (!model.isEmpty) {
        resetLayout();
    }

    $: diagram.dataOptions = dataOptions;
    $: diagram.layoutOptions = layoutOptions;
    $: diagram.glyphOptions = glyphOptions;

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

    export function highlight(searchText: string | null, searchMode: SearchMode) {
        diagram.highlight(searchText, searchMode);
    }

    export function isolate(searchText: string | null, searchMode: SearchMode) {
        diagram.isolate(searchText, searchMode);
    }

    export function reset() {
        diagram.reset();
    }
</script>

<div
    bind:this={loadingScreenElement}
    class="loading-screen w-100p overflow-hidden h-100p absolute z-1 flex flex-col align-items-center justify-content-center bg-surface-50"
    class:hidden={status !== StructuralStatus.RunningInBackground}
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
