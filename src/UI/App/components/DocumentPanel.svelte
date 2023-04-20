<script lang="ts">
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import type { VisualizationModel } from "model/visualization";

    export let model: VisualizationModel;

    $: items = [
        { key: "Name", value: model.documentInfo.name },
        {
            key: "CreatedOn",
            value: new Date(model.documentInfo.createdOn).toLocaleString(),
        },
        { key: "Revision", value: model.documentInfo.revision },
        { key: "HelvegVersion", value: model.documentInfo.helvegVersion },
        { key: "NodeCount", value: Object.keys(model.multigraph.nodes).length.toString() },
    ];
</script>

<Panel name="Document">
    <KeyValueList {items} />
</Panel>
