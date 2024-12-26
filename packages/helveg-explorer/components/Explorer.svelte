<script lang="ts">
    import { get } from "svelte/store";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import AppearancePanel from "./AppearancePanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { onMount, setContext } from "svelte";
    import Toast from "./Toast.svelte";
    import ToolBox from "./ToolBox.svelte";
    import ToolsPanel from "./ToolsPanel.svelte";
    import { type Diagram } from "../deps/helveg-diagram.ts";
    import { AppIcons, AppPanels, AppTools } from "../const.ts";
    import LoadingScreen from "./LoadingScreen.svelte";
    import SearchPanel from "./SearchPanel.svelte";
    import { createExplorerState } from "../explorer-state.ts";
    import ContextMenu from "./ContextMenu.svelte";

    export let rootElement: HTMLElement;
    setContext("rootElement", rootElement);
    export let diagram: Diagram;
    setContext("diagram", diagram);

    const state = createExplorerState(rootElement, diagram);
    setContext("state", state);
    setContext("model", state.model);
    setContext("logger", state.logger);
    setContext("dataOptions", state.dataOptions);
    setContext("layoutOptions", state.layoutOptions);
    setContext("appearanceOptions", state.appearanceOptions);
    setContext("exportOptions", state.exportOptions);
    setContext("toolOptions", state.toolOptions);
    const { status, stats, selectedTool, dataOptions } = state;

    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let searchResults: string[];

    function onNodeSelected(nodeId: string | null) {
        if (nodeId === null) {
            propertiesPanel.$set({
                node: null,
            });
            diagram.highlightNode(null, false, false);
            return;
        }

        switch ($selectedTool) {
            case AppTools.ShowProperties:
                propertiesPanel.$set({
                    node: diagram.model.data?.nodes[nodeId] ?? null,
                });
                dock.setTab(AppPanels.Properties);
                diagram.highlightNode(
                    nodeId,
                    get(state.toolOptions).showProperties
                        .shouldHighlightSubtree,
                    get(state.toolOptions).showProperties
                        .shouldHighlightNeighbors,
                );
                break;
        }
    }
    diagram.events.nodeSelected.subscribe(onNodeSelected);

    function onNodeClicked(nodeId: string) {
        switch ($selectedTool) {
            case AppTools.Toggle:
                diagram.toggleNode(nodeId);
                break;
            case AppTools.Cut:
                diagram.cut(nodeId, get(state.toolOptions).cut);
                break;
        }
    }
    diagram.events.nodeClicked.subscribe(onNodeClicked);

    selectedTool.subscribe((tool) => {
        diagram.canDragNodes = tool == AppTools.Move;

        switch (tool) {
            case AppTools.Cut:
                dock.setTab(AppPanels.Tools);
                break;
        }
    });

    state.appearanceOptions.subscribe((v) => {
        const glyphOptions = diagram.glyphProgramOptions;
        glyphOptions.isFireAnimated = v.glyph.isFireAnimated;
        glyphOptions.showFire = v.glyph.showFire;
        glyphOptions.showIcons = v.glyph.showIcons;
        glyphOptions.showLabels = v.glyph.showLabels;
        glyphOptions.showOutlines = v.glyph.showOutlines;
        glyphOptions.showDiffs = v.glyph.showDiffs;
        glyphOptions.dimCollapsedNodes = v.glyph.dimCollapsedNodes;
        glyphOptions.sizingMode = v.glyph.sizingMode;

        glyphOptions.crustWidth = v.codePizza.crustWidth;
        glyphOptions.sauceWidth = v.codePizza.sauceWidth;
        glyphOptions.isPizzaEnabled = v.codePizza.isEnabled;
        glyphOptions.pizzaToppings =
            v.codePizza.pizzaToppings ?? glyphOptions.pizzaToppings;
        diagram.glyphProgramOptions = glyphOptions;
    });
</script>

<div
    class="explorer-svelte flex flex-row-reverse h-100p relative pointer-events-none"
    bind:this={rootElement}
>
    <div class="diagram-background" />

    <LoadingScreen status={$status} />

    <ToolBox bind:selectedTool={$selectedTool} class="z-1" />

    <Dock
        name="panels"
        bind:this={dock}
        class="z-2"
        fallbackTab={AppPanels.Guide}
    >
        <Tab name="Search" value={AppPanels.Search} icon={AppIcons.SearchPanel}>
            <SearchPanel
                on:highlight={(e) =>
                    (searchResults = diagram.highlight(
                        e.detail.searchText,
                        e.detail.searchMode,
                        e.detail.expandedOnly,
                        e.detail.filterBuilder,
                    ))}
                on:isolate={(e) =>
                    diagram.isolate(
                        e.detail.searchText,
                        e.detail.searchMode,
                        e.detail.filterBuilder,
                    )}
                on:selected={(e) => (diagram.selectedNode = e.detail)}
                results={searchResults}
            />
        </Tab>
        <Tab name="Layout" value={AppPanels.Layout} icon={AppIcons.LayoutPanel}>
            <LayoutPanel
                on:run={(e) => diagram.runLayout(e.detail)}
                on:stop={() => diagram.stopLayout()}
                on:tidyTree={() => diagram.resetLayout()}
                on:refresh={() =>
                    diagram.refresh({
                        selectedRelations: $dataOptions.selectedRelations,
                        selectedKinds: $dataOptions.selectedKinds,
                        expandedDepth: $dataOptions.expandedDepth,
                    })}
                status={$status}
                stats={$stats}
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
    <ContextMenu />
</div>
