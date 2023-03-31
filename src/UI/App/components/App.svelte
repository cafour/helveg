<script lang="ts">
    import { getIconDataUrl } from "model/icons";
    import type { VisualizationModel } from "model/visualization";
    import Diagram from "./Diagram.svelte";
    import Dock from "./Dock.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
    import Tab from "./Tab.svelte";

    const dataId = "helveg-data";

    const dataScript = document.getElementById(dataId);
    if (dataScript == null) {
        throw new Error(`Could not find the '${dataId}' element.`);
    }

    const data = <VisualizationModel>JSON.parse(dataScript.textContent!);
</script>

<main class="flex flex-row-reverse h-100p relative">
    <Diagram model={data}/>

    <Dock name="panels">
        <Tab name="Data" value="data-panel" icon="base:Database">
            <strong>TODO: Data Panel Contents</strong>
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
            <strong>TODO: Properties Panel Contents</strong>
        </Tab>
        <Tab name="Document" value="document-panel" icon="base:Document">
            <DocumentPanel documentInfo={data.documentInfo}/>
        </Tab>
    </Dock>
</main>
