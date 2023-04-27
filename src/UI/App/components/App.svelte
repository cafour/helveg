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
    import AppearancePanel from "./AppearancePanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { writable, type Readable } from "svelte/store";
    import type { HelvegInstance } from "model/instance";
    import { setContext } from "svelte";
    import { AppIcons, AppPanels, AppTools } from "model/const";
    import Toast from "./Toast.svelte";
    import ToolBox from "./ToolBox.svelte";

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
    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let status: StructuralStatus;
    let selectedNodeId: string | null;
    let stats: StructuralDiagramStats;
    let selectedTool: string;

    function onNodeSelected(nodeId: string) {
        switch (selectedTool) {
            case AppTools.ShowProperties:
                propertiesPanel.$set({node: instance.model.multigraph.nodes[nodeId] ?? null})
                dock.setTab(AppPanels.Properties);
                break;
            case AppTools.Cut:
                diagram.cut(nodeId);
                break;
        }
    }
</script>

<main class="flex flex-row-reverse h-100p relative">
    <ToolBox bind:selectedTool />

    <StructuralDiagramWrapper
        {model}
        bind:this={diagram}
        bind:status
        bind:selectedNodeId
        bind:stats
        bind:dataOptions={$dataOptions}
        bind:glyphOptions={$glyphOptions}
        bind:layoutOptions={$layoutOptions}
        canDragNodes={selectedTool === AppTools.Move}
        on:nodeSelected={(e) => onNodeSelected(e.detail)}
    />

    <Dock name="panels" bind:this={dock}>
        <Tab name="Data" value={AppPanels.Data} icon={AppIcons.DataPanel}>
            <DataPanel
                bind:dataOptions={$dataOptions}
                on:reset={diagram.reset}
                on:highlight={(e) =>
                    diagram.highlight(e.detail.searchText, e.detail.searchMode)}
                on:isolate={(e) =>
                    diagram.isolate(e.detail.searchText, e.detail.searchMode)}
            />
        </Tab>
        <Tab name="Layout" value={AppPanels.Layout} icon={AppIcons.LayoutPanel}>
            <LayoutPanel
                bind:layoutOptions={$layoutOptions}
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
            <AppearancePanel bind:glyphOptions={$glyphOptions} />
        </Tab>
        <Tab
            name="Properties"
            value={AppPanels.Properties}
            icon={AppIcons.PropertiesPanel}
        >
            <PropertiesPanel
                bind:this={propertiesPanel}
            />
        </Tab>
        <Tab
            name="Document"
            value={AppPanels.Document}
            icon={AppIcons.DocumentPanel}
        >
            <DocumentPanel
                {model}
                bind:exportOptions={$exportOptions}
                on:export={(e) => diagram.save(e.detail)}
            />
        </Tab>
        <Tab name="Guide" value={AppPanels.Guide} icon={AppIcons.GuidePanel}>
            <GuidePanel />
        </Tab>
    </Dock>

    <Toast />
</main>
