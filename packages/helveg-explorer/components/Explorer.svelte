<script lang="ts">
    import { writable, readable, get, type Writable } from "svelte/store";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DataPanel from "./DataPanel.svelte";
    import AppearancePanel from "./AppearancePanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { setContext } from "svelte";
    import Toast from "./Toast.svelte";
    import ToolBox from "./ToolBox.svelte";
    import ToolsPanel from "./ToolsPanel.svelte";
    import type { Diagram } from "../deps/helveg-diagram.ts";
    import { AppIcons, AppPanels, AppTools } from "../const.ts";
    import * as Options from "../options.ts";

    export let diagram: Diagram;
    setContext("diagram", diagram);

    const status = readable(diagram.status, (set) => {
        diagram.events.statusChanged.subscribe(set);
        return () => diagram.events.statusChanged.unsubscribe(set);
    });

    const stats = readable(diagram.stats, (set) => {
        diagram.events.statsChanged.subscribe(set);
        return () => diagram.events.statsChanged.unsubscribe(set);
    });

    function createOptions<T>(
        contextName: string,
        storageName: string,
        defaults: T
    ): Writable<T> {
        const options = writable(
            Options.loadOptions<T>(storageName) ?? defaults
        );
        options.subscribe((v) => Options.saveOptions<T>(storageName, v));
        setContext(contextName, options);
        return options;
    }

    const dataOptions = createOptions<Options.DataOptions>(
        "dataOptions",
        "data",
        Options.DEFAULT_DATA_OPTIONS
    );
    const layoutOptions = createOptions<Options.LayoutOptions>(
        "layoutOptions",
        "layout",
        Options.DEFAULT_LAYOUT_OPTIONS
    );
    const appearanceOptions = createOptions<Options.AppearanceOptions>(
        "appearanceOptions",
        "appearance",
        Options.DEFAULT_APPEARANCE_OPTIONS
    );
    const exportOptions = createOptions<Options.ExportOptions>(
        "exportOptions",
        "export",
        helveg.DEFAULT_EXPORT_OPTIONS
    );
    const toolOptions = createOptions<Options.ToolOptions>(
        "toolOptions",
        "tool",
        Options.DEFAULT_TOOL_OPTIONS
    );

    const model = readable(diagram.model, (set) => {
        diagram.events.modelChanged.subscribe(set);
        return () => diagram.events.modelChanged.unsubscribe(set);
    });
    setContext("model", model);

    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
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
                    node: diagram.model.multigraph.nodes[nodeId] ?? null,
                });
                dock.setTab(AppPanels.Properties);
                diagram.highlightNode(
                    nodeId,
                    get(toolOptions).showProperties.shouldHighlightSubtree,
                    get(toolOptions).showProperties.shouldHighlightNeighbors
                );
                break;
        }
    }
    diagram.events.nodeSelected.subscribe(onNodeSelected);

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
    diagram.events.nodeClicked.subscribe(onNodeClicked);

    function onToolChanged(tool: string) {
        switch (tool) {
            case AppTools.Cut:
                dock.setTab(AppPanels.Tools);
                break;
        }
    }
</script>

<div class="flex flex-row-reverse h-100p relative"></div>
    <ToolBox bind:selectedTool on:change={() => onToolChanged(selectedTool)} />

    <Dock name="panels" bind:this={dock}>
        <Tab name="Data" value={AppPanels.Data} icon={AppIcons.DataPanel}>
            <DataPanel
                on:refresh={() =>
                    diagram.refresh({ selectedRelations: $dataOptions.selectedRelations })}
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
</div>
