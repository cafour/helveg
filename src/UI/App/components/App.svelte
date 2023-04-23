<script lang="ts">
    import "model/instance";
    import StructuralDiagramWrapper from "./StructuralDiagramWrapper.svelte";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import type { StructuralDiagramStats, StructuralStatus } from "model/structural";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DataPanel from "./DataPanel.svelte";
    import GlyphsPanel from "./GlyphsPanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { DEFAULT_DATA_OPTIONS, DEFAULT_EXPORT_OPTIONS, DEFAULT_GLYPH_OPTIONS, DEFAULT_LAYOUT_OPTIONS, type DataOptions, type ExportOptions, type GlyphOptions, type LayoutOptions } from "model/options";
    import type { GraphNode } from "model/multigraph";
    import type { Readable } from "svelte/store";

    let model = helveg.model;
    helveg.modelLoaded.subscribe(() => {
        DEBUG && console.log(`Model '${helveg.model.multigraph.label}' loaded in App.`);
        model = helveg.model;
    });

    let diagram: StructuralDiagramWrapper;
    let status: Readable<StructuralStatus>;
    let stats: Readable<StructuralDiagramStats>;
    let dataOptions: DataOptions = DEFAULT_DATA_OPTIONS;
    let layoutOptions: LayoutOptions = DEFAULT_LAYOUT_OPTIONS;
    let glyphOptions: GlyphOptions = DEFAULT_GLYPH_OPTIONS;
    let exportOptions: ExportOptions = DEFAULT_EXPORT_OPTIONS;
    let selectedNode: GraphNode | null = null;
</script>

<main class="flex flex-row-reverse h-100p relative">
    <StructuralDiagramWrapper
        {model}
        bind:this={diagram}
        bind:stats
        bind:status
    />

    <Dock name="panels">
        <Tab name="Data" value="data-panel" icon="base:Database">
            <DataPanel bind:options={dataOptions} />
        </Tab>
        <Tab name="Layout" value="layout-panel" icon="base:Diagram">
            <LayoutPanel
                bind:options={layoutOptions}
                on:run={(e) => diagram.runLayout(e.detail)}
                on:stop={diagram.stopLayout}
                on:tidyTree={diagram.resetLayout}
                status={$status}
                stats={$stats}
            />
        </Tab>
        <Tab name="Glyphs" value="glyphs-panel" icon="base:PolarChart">
            <GlyphsPanel bind:options={glyphOptions} />
        </Tab>
        <Tab
            name="Properties"
            value="properties-panel"
            icon="base:ShowAllConfigurations"
        >
            <PropertiesPanel node={selectedNode} />
        </Tab>
        <Tab name="Document" value="document-panel" icon="base:Document">
            <DocumentPanel
                model={model}
                bind:exportOptions={exportOptions}
                on:export={(e) => diagram.save(e.detail)}
            />
        </Tab>
        <Tab name="Guide" value="guide-panel" icon="base:StatusHelpOutline">
            <GuidePanel />
        </Tab>
    </Dock>
</main>
