<script lang="ts">
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { createEventDispatcher, getContext } from "svelte";
    import { AppPanels } from "model/const";
    import type { Readable, Writable } from "svelte/store";
    import type { VisualizationModel } from "model/visualization";
    import type { ExportOptions } from "model/options";
    import type { HelvegInstance } from "types";
    import FileSaver from "file-saver";

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

    let instance = getContext<HelvegInstance>("helveg");
    let model = getContext<Readable<VisualizationModel>>("model");
    let exportOptions = getContext<Writable<ExportOptions>>("exportOptions");

    let importStateInput: HTMLInputElement | null = null;

    function importState(target: EventTarget | null) {
        let fileUpload = target as HTMLInputElement;
        if (!fileUpload?.files) {
            return;
        }
        
        if (fileUpload.files.length === 0) {
            return;
        }

        var reader = new FileReader();
        reader.readAsText(fileUpload.files[fileUpload.files.length - 1]);

        reader.onload = (e) => {
            let result = e.target?.result;
            if (typeof result !== "string") {
                return;
            }
            
            let state = JSON.parse(result);
            if (state) {
                instance.importState(state);
            }
        };
    }
    
    function exportState() {
        let state = instance.exportState();
        if (state) {
            FileSaver.saveAs(
                new Blob([JSON.stringify(state)],
                { type: "application/json;charset=utf-8" }),
                `${instance.model.documentInfo.name}.${new Date().toISOString().slice(0, 10)}.helveg-state.json`);
        }
    }
</script>

<Panel name="Document" indent={false} id={AppPanels.Document}>
    <Subpanel name="Metadata">
        <KeyValueList items={metadataItems} />
    </Subpanel>
    <Subpanel name="State">
        <button class="button-stretch" on:click|preventDefault={() => instance.resetOptions()}>
            Reset options
        </button>
        <input type="file" bind:this={importStateInput} style="display: none" on:change={e => importState(e.target)}/>
        <button class="button-stretch" on:click|preventDefault={() => importStateInput?.click()}>
            Import state
        </button>
        <button class="button-stretch" on:click|preventDefault={() => exportState()}>
            Export state
        </button>
    </Subpanel>
    <Subpanel name="Export image">
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
        
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeEffects}
            />
            IncludeEffects
        </label>
        
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includePizzaDough}
            />
            IncludePizzaDough
        </label>
        
        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeHighlights}
            />
            IncludeHighlights
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
        
        <label class="flex flex-row gap-8 align-items-center">
            Opacity
            <input type="range" id="alpha" min="0" max="1" step="0.1" bind:value={$exportOptions.opacity}>
        </label>
        
        <button class="button-stretch" on:click|preventDefault={() => dispatch("export", $exportOptions)}>
            Export
        </button>
    </Subpanel>
</Panel>
