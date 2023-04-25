<script lang="ts">
    import "model/instance";
    import StructuralDiagramWrapper from "./StructuralDiagramWrapper.svelte";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import type {
        StructuralDiagramStats,
        StructuralStatus,
    } from "model/structural";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DataPanel from "./DataPanel.svelte";
    import GlyphsPanel from "./GlyphsPanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { writable, type Readable } from "svelte/store";
    import type { HelvegInstance } from "model/instance";
    import { setContext } from "svelte";
    import { AppIcons } from "model/const";

    export let instance: HelvegInstance;
    setContext("helveg", instance);

    let model = instance.model;
    instance.loaded.subscribe(() => {
        DEBUG &&
            console.log(
                `Model '${instance.model.multigraph.label}' loaded in App.`
            );
        model = instance.model;
    });

    let dataOptions = writable(helveg.options.data);
    let layoutOptions = writable(helveg.options.layout);
    let glyphOptions = writable(helveg.options.glyph);
    let exportOptions = writable(helveg.options.export);
    let diagram: StructuralDiagramWrapper;
    let status: StructuralStatus;
    let selectedNodeId: string | null;
    let stats: StructuralDiagramStats;
</script>

<main class="flex flex-row-reverse h-100p relative">
    <StructuralDiagramWrapper
        {model}
        bind:this={diagram}
        bind:status
        bind:selectedNodeId
        bind:stats
        bind:dataOptions={$dataOptions}
        bind:glyphOptions={$glyphOptions}
        bind:layoutOptions={$layoutOptions}
    />

    <Dock name="panels">
        <Tab name="Data" value="data-panel" icon={AppIcons.DataPanel}>
            <DataPanel bind:dataOptions={$dataOptions} />
        </Tab>
        <Tab name="Layout" value="layout-panel" icon={AppIcons.LayoutPanel}>
            <LayoutPanel
                bind:layoutOptions={$layoutOptions}
                on:run={(e) => diagram.runLayout(e.detail)}
                on:stop={diagram.stopLayout}
                on:tidyTree={diagram.resetLayout}
                {status}
                {stats}
            />
        </Tab>
        <Tab name="Glyphs" value="glyphs-panel" icon={AppIcons.GlyphsPanel}>
            <GlyphsPanel bind:glyphOptions={$glyphOptions} />
        </Tab>
        <Tab
            name="Properties"
            value="properties-panel"
            icon={AppIcons.PropertiesPanel}
        >
            <PropertiesPanel
                node={selectedNodeId
                    ? instance.model.multigraph.nodes[selectedNodeId]
                    : null}
            />
        </Tab>
        <Tab
            name="Document"
            value="document-panel"
            icon={AppIcons.DocumentPanel}
        >
            <DocumentPanel
                {model}
                bind:exportOptions={$exportOptions}
                on:export={(e) => diagram.save(e.detail)}
            />
        </Tab>
        <Tab name="Guide" value="guide-panel" icon={AppIcons.GuidePanel}>
            <GuidePanel />
        </Tab>
    </Dock>
</main>
