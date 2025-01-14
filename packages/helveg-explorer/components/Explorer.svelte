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
    import { type Diagram, type ModifierKeyStateChange } from "../deps/helveg-diagram.ts";
    import { AppIcons, AppPanels, AppTools } from "../const.ts";
    import LoadingScreen from "./LoadingScreen.svelte";
    import SearchPanel from "./SearchPanel.svelte";
    import { createExplorerState } from "../explorer-state.ts";
    import ContextMenu from "./ContextMenu.svelte";
    import TreeView from "./TreeView.svelte";

    export let rootElement: HTMLElement;
    setContext("rootElement", rootElement);
    export let diagram: Diagram;
    setContext("diagram", diagram);

    diagram.cursorOptions = {
        defaultCursor: "default",
        hoverCursor: "pointer",
        shiftHoverCursor: "move",
        altHoverCursor: `url("${diagram.options.iconRegistry.getIconDataUrl("helveg:cursor-trash")}"), not-allowed`,
    };

    const state = createExplorerState(rootElement, diagram);
    setContext("state", state);
    setContext("model", state.model);
    setContext("logger", state.logger);
    setContext("dataOptions", state.dataOptions);
    setContext("layoutOptions", state.layoutOptions);
    setContext("appearanceOptions", state.appearanceOptions);
    setContext("exportOptions", state.exportOptions);
    setContext("toolOptions", state.toolOptions);
    const { status, stats, selectedTool, selectedNode, dataOptions } = state;

    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let searchResults: string[];

    selectedNode.subscribe(async (nodeId) => {
        if (!propertiesPanel || !dock) {
            return;
        }

        if (nodeId === null) {
            propertiesPanel.$set({
                node: null,
            });
            await diagram.highlightNode(null, false, false);
            return;
        }

        switch ($selectedTool) {
            case AppTools.ShowProperties:
                propertiesPanel.$set({
                    node: diagram.model.data?.nodes[nodeId] ?? null,
                });
                dock.setTab(AppPanels.Properties);
                await diagram.highlightNode(
                    nodeId,
                    get(state.toolOptions).showProperties
                        .shouldHighlightSubtree,
                    get(state.toolOptions).showProperties
                        .shouldHighlightNeighbors,
                );
                break;
        }
    });

    function onDiagramNodeClicked(nodeId: string) {
        if (diagram.modifierKeyState.alt && !diagram.modifierKeyState.control && !diagram.modifierKeyState.shift) {{
            diagram.remove(nodeId, get(state.toolOptions).remove);
        }}
        
        if (!diagram.modifierKeyState.alt && !diagram.modifierKeyState.control && !diagram.modifierKeyState.shift) {
            diagram.selectedNode = nodeId;
        }
    }
    diagram.events.nodeClicked.subscribe(onDiagramNodeClicked);
    
    function onModifierKeysChanged(change: ModifierKeyStateChange) {
        diagram.canDragNodes = change.new.shift;
    }
    diagram.events.modifierKeysChanged.subscribe(onModifierKeysChanged);

    function onDiagramNodeDoubleClicked(nodeId: string) {
        if (!diagram.modifierKeyState.alt && !diagram.modifierKeyState.control && !diagram.modifierKeyState.shift) {
            diagram.toggleNode(nodeId);
        }
    }
    diagram.events.nodeDoubleClicked.subscribe(onDiagramNodeDoubleClicked);

    selectedTool.subscribe((tool) => {
        diagram.canDragNodes = tool == AppTools.Move;
    });
</script>

<div
    class="explorer-svelte flex flex-row h-100p relative pointer-events-none justify-content-between"
    bind:this={rootElement}
>
    <div class="diagram-background"></div>

    <LoadingScreen status={$status} />

    <TreeView
        class="z-2"
        bind:selectedNode={$selectedNode}
        on:nodeClicked={() => ($selectedTool = AppTools.ShowProperties)}
    />

    <ToolBox bind:selectedTool={$selectedTool} class="z-1" />

    <!-- filler element -->
    <div class="filler flex-grow-1"></div>

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
                on:selected={(e) => {
                    $selectedTool = AppTools.ShowProperties;
                    diagram.selectedNode = e.detail;
                }}
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
