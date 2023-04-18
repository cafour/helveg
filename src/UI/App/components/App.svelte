<script lang="ts" context="module">
    import { writable, type Readable, readable } from "svelte/store";

    export let model = readable(helveg.model, (set) => {
        helveg.loadingPromise.then((_) => {
            set(helveg.model);
        });
        return () => {};
    });
</script>

<script lang="ts">
    import "model/global";
    import StructuralDiagram from "./StructuralDiagram.svelte";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import { StructuralState } from "model/structural";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DataPanel from "./DataPanel.svelte";
    import CSharpPlugin from "model/csharpPlugin";
    import GlyphsPanel from "./GlyphsPanel.svelte";
    import LayoutPanel from "./LayoutPanel.svelte";

    let initialState = new StructuralState();
    initialState.applyPlugin(new CSharpPlugin());
    let state = writable(initialState);
    let diagram: StructuralDiagram;
    let iterations: number;
    let speed: number;
</script>

<main class="flex flex-row-reverse h-100p relative">
    <StructuralDiagram
        model={$model}
        bind:state={$state}
        bind:this={diagram}
        bind:iterations
        bind:speed
    />

    <Dock name="panels">
        <Tab name="Data" value="data-panel" icon="base:Database">
            <DataPanel bind:options={$state.dataOptions} />
        </Tab>
        <Tab name="Layout" value="layout-panel" icon="base:Diagram">
            <LayoutPanel
                bind:options={$state.layoutOptions}
                status={$state.status}
                on:run={(e) => diagram.run(e.detail)}
                on:stop={diagram.stop}
                {iterations}
                {speed}
            />
        </Tab>
        <Tab name="Glyphs" value="glyphs-panel" icon="base:PolarChart">
            <GlyphsPanel bind:options={$state.glyphOptions} />
        </Tab>
        <Tab
            name="Properties"
            value="properties-panel"
            icon="base:ShowAllConfigurations"
        >
            <PropertiesPanel node={$state.selectedNode} />
        </Tab>
        <Tab name="Document" value="document-panel" icon="base:Document">
            <DocumentPanel documentInfo={$model.documentInfo} />
        </Tab>
    </Dock>
</main>
