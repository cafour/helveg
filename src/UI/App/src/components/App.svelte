<script lang="ts">
    import { writable, readable, get } from "svelte/store";
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
    import { type DataOptions, saveOptions, type LayoutOptions, type ExportOptions, type ToolOptions, type AbstractStructuralDiagram, type AppearanceOptions } from "types";

    export let instance: HelvegInstance;
    setContext("helveg", instance);

    let dataOptions = writable(instance.options.data, set => {
        instance.optionsChanged.subscribe(o => set(o.data));
    });
    dataOptions.subscribe(o => saveOptions<DataOptions>("data", o));
    setContext("dataOptions", dataOptions);

    let layoutOptions = writable(instance.options.layout, set => {
        instance.optionsChanged.subscribe(o => set(o.layout));
    });
    layoutOptions.subscribe(o => saveOptions<LayoutOptions>("layout", o));
    setContext("layoutOptions", layoutOptions);

    let appearanceOptions = writable(instance.options.appearance, set => {
        instance.optionsChanged.subscribe(o => set(o.appearance));
    });
    appearanceOptions.subscribe(o => saveOptions<AppearanceOptions>("appearance", o));
    setContext("appearanceOptions", appearanceOptions);

    let exportOptions = writable(instance.options.export, set => {
        instance.optionsChanged.subscribe(o => set(o.export));
    });
    exportOptions.subscribe(o => saveOptions<ExportOptions>("export", o));
    setContext("exportOptions", exportOptions);

    let toolOptions = writable(instance.options.tool, set => {
        instance.optionsChanged.subscribe(o => set(o.tool));
    });
    toolOptions.subscribe(o => saveOptions<ToolOptions>("tool", o));
    setContext("toolOptions", toolOptions);

    let model = readable(instance.model, (set) => {
        instance.loaded.subscribe((model) => {
            set(model);
        });
    });
    setContext("model", model);

    let diagram: StructuralDiagramWrapper;
    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let status: StructuralStatus;
    let selectedNodeId: string | null;
    let stats: StructuralDiagramStats;
    let selectedTool: string;

    function onNodeSelected(nodeId: string | null) {
        if (nodeId === null) {
            propertiesPanel.$set({
                node: null,
            });
            diagram.highlightNode(null, false, false);
            return;
        }
        
        switch (selectedTool) {
            case AppTools.ShowProperties:
                propertiesPanel.$set({
                    node: instance.model.multigraph.nodes[nodeId] ?? null,
                });
                dock.setTab(AppPanels.Properties);
                diagram.highlightNode(
                    nodeId,
                    get(toolOptions).showProperties.shouldHighlightSubtree,
                    get(toolOptions).showProperties.shouldHighlightNeighbors);
                break;
        }
    }

    function onNodeClicked(nodeId: string) {
        switch (selectedTool) {
            case AppTools.Toggle:
                diagram.toggleNode(nodeId);
                break;
            case AppTools.Cut:
                diagram.cut(nodeId);
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
    
    export function getDiagram(): AbstractStructuralDiagram {
        return diagram.getDiagram();
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
