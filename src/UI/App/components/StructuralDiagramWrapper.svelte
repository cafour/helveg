<script lang="ts">
    import type { VisualizationModel } from "model/visualization";
    import { StructuralStatus, DefaultStructuralDiagram, type StructuralDiagram } from "model/structural";
    import Icon from "./Icon.svelte";
    import { readable } from "svelte/store";
    import type { ExportOptions } from "model/options";

    export let model: VisualizationModel;

    let diagram: StructuralDiagram = new DefaultStructuralDiagram(helveg.glyphStyleRepository);

    export const status = readable(diagram.status, set => {
        diagram.statusChanged.subscribe(set);
    });
    export const stats = readable(diagram.stats, set => {
        diagram.statsChanged.subscribe(set);
    });

    let diagramElement: HTMLElement | null = null;
    let loadingScreenElement: HTMLElement;

    $: diagram.element = diagramElement;
    $: diagram.model = model;
    $: if (!model.isEmpty) {
        resetLayout();
    }

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
</script>

<div
    bind:this={loadingScreenElement}
    class="loading-screen w-100p overflow-hidden h-100p absolute z-1 flex flex-col align-items-center justify-content-center bg-surface-50"
    class:hidden={$status !== StructuralStatus.RunningInBackground}
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
