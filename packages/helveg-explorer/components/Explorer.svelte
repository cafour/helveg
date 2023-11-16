<script lang="ts">
    import { writable, readable, get, type Writable } from "svelte/store";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import AppearancePanel from "./AppearancePanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";
    import GuidePanel from "./GuidePanel.svelte";
    import { setContext } from "svelte";
    import Toast from "./Toast.svelte";
    import ToolBox from "./ToolBox.svelte";
    import ToolsPanel from "./ToolsPanel.svelte";
    import {
        sublogger,
        type Diagram,
        createCsharpRelationStylist,
    } from "../deps/helveg-diagram.ts";
    import { AppIcons, AppPanels, AppTools } from "../const.ts";
    import * as Options from "../options.ts";
    import LoadingScreen from "./LoadingScreen.svelte";
    import SearchPanel from "./SearchPanel.svelte";

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

    const logger = sublogger(diagram.logger, "explorer");
    setContext("logger", logger);

    function createOptions<T>(
        contextName: string,
        storageName: string,
        defaults: T
    ): Writable<T> {
        const options = writable({
            ...defaults,
            ...Options.loadOptions<T>(storageName),
        });
        options.subscribe((v) => Options.saveOptions<T>(storageName, v));
        setContext(contextName, options);
        return options;
    }

    const dataOptions = createOptions<Options.DataOptions>(
        "dataOptions",
        "data",
        { ...structuredClone(Options.DEFAULT_DATA_OPTIONS), ...diagram.options.refresh }
    );
    const layoutOptions = createOptions<Options.LayoutOptions>(
        "layoutOptions",
        "layout",
        structuredClone(Options.DEFAULT_LAYOUT_OPTIONS)
    );
    $layoutOptions.tidyTree.relation = diagram.mainRelation;
    layoutOptions.subscribe((v) => {
        diagram.mainRelation = v.tidyTree.relation;
        diagram.forceAtlas2Options = v.forceAtlas2;
    });
    const appearanceOptions = createOptions<Options.AppearanceOptions>(
        "appearanceOptions",
        "appearance",
        { relationColors: {}, ...structuredClone(Options.DEFAULT_APPEARANCE_OPTIONS) }
    );
    $: diagram.relationStylist = createCsharpRelationStylist(
        $appearanceOptions.relationColors!
    );
    const exportOptions = createOptions<Options.ExportOptions>(
        "exportOptions",
        "export",
        structuredClone(Options.DEFAULT_EXPORT_OPTIONS)
    );
    const toolOptions = createOptions<Options.ToolOptions>(
        "toolOptions",
        "tool",
        structuredClone(Options.DEFAULT_TOOL_OPTIONS)
    );

    const model = readable(diagram.model, (set) => {
        diagram.events.modelChanged.subscribe(set);
        return () => diagram.events.modelChanged.unsubscribe(set);
    });
    setContext("model", model);

    let dock: Dock;
    let propertiesPanel: PropertiesPanel;
    let selectedTool: string;
    let searchResults: string[];

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
                    node: diagram.model.data?.nodes[nodeId] ?? null,
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
        diagram.canDragNodes = tool == AppTools.Move;

        switch (tool) {
            case AppTools.Cut:
                dock.setTab(AppPanels.Tools);
                break;
        }
    }

    appearanceOptions.subscribe((v) => {
        const glyphOptions = diagram.glyphProgramOptions;
        glyphOptions.isFireAnimated = v.glyph.isFireAnimated;
        glyphOptions.showFire = v.glyph.showFire;
        glyphOptions.showIcons = v.glyph.showIcons;
        glyphOptions.showLabels = v.glyph.showLabels;
        glyphOptions.showOutlines = v.glyph.showOutlines;
        glyphOptions.showDiffs = v.glyph.showDiffs;

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
>
    <div class="diagram-background" />

    <LoadingScreen status={$status} />

    <ToolBox
        bind:selectedTool
        on:change={() => onToolChanged(selectedTool)}
        class="z-1"
    />

    <Dock name="panels" bind:this={dock} class="z-2">
        <Tab name="Search" value={AppPanels.Search} icon={AppIcons.SearchPanel}>
            <SearchPanel
                on:highlight={(e) =>
                    searchResults = diagram.highlight(e.detail.searchText, e.detail.searchMode)}
                on:isolate={(e) =>
                    diagram.isolate(e.detail.searchText, e.detail.searchMode)}
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
</div>
