<script lang="ts" context="module">
    import type { Readable, Writable } from "svelte/store";
    import type * as opts from "model/options";
    import type { VisualizationModel } from "model/visualization";

    export let dataOptions: Writable<opts.DataOptions> = null!;
    export let layoutOptions: Writable<opts.LayoutOptions> = null!;
    export let glyphOptions: Writable<opts.GlyphOptions> = null!;
    export let exportOptions: Writable<opts.ExportOptions> = null!;
    export let toolOptions: Writable<opts.ToolOptions> = null!;
    export let model: Readable<VisualizationModel> = null!;
</script>

<script lang="ts">
    import { writable, readable } from "svelte/store";
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
    import AppearancePanel from "./AppearancePanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import type { HelvegInstance } from "model/instance";
    import { setContext } from "svelte";
    import { AppIcons, AppPanels, AppTools } from "model/const";
    import Toast from "./Toast.svelte";
    import ToolBox from "./ToolBox.svelte";
    import ToolsPanel from "./ToolsPanel.svelte";

    export let instance: HelvegInstance;
    setContext("helveg", instance);

    dataOptions = writable(instance.options.data);
    layoutOptions = writable(instance.options.layout);
    glyphOptions = writable(instance.options.glyph);
    exportOptions = writable(instance.options.export);
    toolOptions = writable(instance.options.tool);
    model = readable(instance.model, (set) => {
        instance.loaded.subscribe((model) => {
            set(model);
        });
    });

    let diagram: StructuralDiagramWrapper;
    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let status: StructuralStatus;
    let selectedNodeId: string | null;
    let stats: StructuralDiagramStats;
    let selectedTool: string;

    function onNodeSelected(nodeId: string) {
        switch (selectedTool) {
            case AppTools.ShowProperties:
                propertiesPanel.$set({
                    node: instance.model.multigraph.nodes[nodeId] ?? null,
                });
                dock.setTab(AppPanels.Properties);
                break;
            case AppTools.Cut:
                diagram.cut(nodeId);
                break;
        }
    }

    function onNodeClicked(nodeId: string) {
        switch (selectedTool) {
            case AppTools.Toggle:
                diagram.toggleNode(nodeId);
                break;
        }
    }

    function onToolChanged(tool: string) {
        switch (tool) {
            case AppTools.Cut:
                dock.setTab(AppPanels.Tools);
                break;
        }
    }
</script>

<main class="flex flex-row-reverse h-100p relative">
    <ToolBox bind:selectedTool on:change={() => onToolChanged(selectedTool)} />

    <StructuralDiagramWrapper
        bind:this={diagram}
        bind:status
        bind:selectedNodeId
        bind:stats
        canDragNodes={selectedTool === AppTools.Move}
        on:nodeSelected={(e) => onNodeSelected(e.detail)}
        on:nodeClicked={(e) => onNodeClicked(e.detail)}
    />

    <Dock name="panels" bind:this={dock}>
        <Tab name="Data" value={AppPanels.Data} icon={AppIcons.DataPanel}>
            <DataPanel
                on:refresh={diagram.refresh}
                on:highlight={(e) =>
                    diagram.highlight(e.detail.searchText, e.detail.searchMode)}
                on:isolate={(e) =>
                    diagram.isolate(e.detail.searchText, e.detail.searchMode)}
            />
        </Tab>
        <Tab name="Layout" value={AppPanels.Layout} icon={AppIcons.LayoutPanel}>
            <LayoutPanel
                on:run={(e) => diagram.runLayout(e.detail)}
                on:stop={diagram.stopLayout}
                on:tidyTree={diagram.resetLayout}
                {status}
                {stats}
            />
        </Tab>
        <Tab
            name="Appearance"
            value={AppPanels.Appearance}
            icon={AppIcons.AppearancePanel}
        >
            <AppearancePanel />
        </Tab>
        <Tab name="Tools" value={AppPanels.Tools} icon={AppIcons.ToolsPanel}>
            <ToolsPanel />
        </Tab>
        <Tab
            name="Properties"
            value={AppPanels.Properties}
            icon={AppIcons.PropertiesPanel}
        >
            <PropertiesPanel bind:this={propertiesPanel} />
        </Tab>
        <Tab
            name="Document"
            value={AppPanels.Document}
            icon={AppIcons.DocumentPanel}
        >
            <DocumentPanel on:export={(e) => diagram.save(e.detail)} />
        </Tab>
        <Tab name="Guide" value={AppPanels.Guide} icon={AppIcons.GuidePanel}>
            <GuidePanel />
        </Tab>
    </Dock>

    <Toast />
</main>
