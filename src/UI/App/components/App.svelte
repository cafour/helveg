<script lang="ts">
    import type { VisualizationModel } from "model/visualization";
    import StructuralDiagram from "./StructuralDiagram.svelte";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";
    import { writable } from "svelte/store";
    import { StructuralState } from "model/structural";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DataPanel from "./DataPanel.svelte";

    const dataId = "helveg-data";

    const dataScript = document.getElementById(dataId);
    if (dataScript == null) {
        throw new Error(`Could not find the '${dataId}' element.`);
    }

    const data = <VisualizationModel>JSON.parse(dataScript.textContent!);

    let initialState = new StructuralState();
    initialState.dataOptions.kinds = [
        "csharp:Solution",
        "csharp:Project",
    ];
    let state = writable(initialState);
</script>

<main class="flex flex-row-reverse h-100p relative">
    <StructuralDiagram model={data} bind:state={$state} />

    <Dock name="panels">
        <Tab name="Data" value="data-panel" icon="base:Database">
            <DataPanel bind:options={$state.dataOptions} />
        </Tab>
        <Tab name="Layout" value="layout-panel" icon="base:Diagram">
            <strong>TODO: Layout Panel Contents</strong>
        </Tab>
        <Tab name="Glyphs" value="glyphs-panel" icon="base:PolarChart">
            <strong>TODO: Glyphs Panel Contents</strong>
        </Tab>
        <Tab
            name="Properties"
            value="properties-panel"
            icon="base:ShowAllConfigurations"
        >
            <PropertiesPanel bind:node={$state.selectedNode} />
        </Tab>
        <Tab name="Document" value="document-panel" icon="base:Document">
            <DocumentPanel documentInfo={data.documentInfo} />
        </Tab>
    </Dock>
</main>
