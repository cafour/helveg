<script lang="ts">
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { createEventDispatcher } from "svelte";
    import { exportOptions, model } from "./App.svelte";

    $: metadataItems = [
        { key: "Name", value: $model.documentInfo.name },
        {
            key: "CreatedOn",
            value: new Date($model.documentInfo.createdOn).toLocaleString(),
        },
        { key: "Revision", value: $model.documentInfo.revision },
        { key: "HelvegVersion", value: $model.documentInfo.helvegVersion },
        {
            key: "NodeCount",
            value: Object.keys($model.multigraph.nodes).length.toString(),
        },
    ];

    let dispatch = createEventDispatcher();
</script>

<Panel name="Document" indent={false}>
    <Subpanel name="Metadata">
        <KeyValueList items={metadataItems} />
    </Subpanel>
    <Subpanel name="Export">
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeNodes}
            />
            IncludeNodes
        </label>
        
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeEdges}
            />
            IncludeEdges
        </label>
        
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeLabels}
            />
            IncludeLabels
        </label>
        
        <label class="flex flex-row gap-8 align-items-center">
            Scale
            <input
                type="number"
                min="1"
                bind:value={$exportOptions.scale}
            />
        </label>
        
        <label class="flex flex-row gap-8 align-items-center">
            BackgroundColor
            <input
                type="color"
                bind:value={$exportOptions.backgroundColor}
            />
        </label>
        
        <button class="button-stretch" on:click|preventDefault={() => dispatch("export", exportOptions)}>
            Export
        </button>
    </Subpanel>
</Panel>
