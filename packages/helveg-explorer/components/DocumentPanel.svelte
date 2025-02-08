<script lang="ts">
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { createEventDispatcher, getContext } from "svelte";
    import { AppPanels } from "../const.ts";
    import type { Readable, Writable } from "svelte/store";
    import {
        type DataModel,
        type Diagram,
        saveAs,
    } from "../deps/helveg-diagram.ts";
    import * as Options from "../options.ts";
    import { version } from "../package.json";

    $: metadataItems = [
        { key: "Name", value: $model.name },
        {
            key: "CreatedOn",
            value: new Date($model.createdOn).toLocaleString(),
        },
        { key: "AnalyzerName", value: $model.analyzer.name },
        { key: "AnalyzerVersion", value: $model.analyzer.version },
        { key: "ExplorerVersion", value: version },
        {
            key: "NodeCount",
            value: $model.data ? Object.keys($model.data.nodes).length.toString() : "0",
        },
    ];

    let dispatch = createEventDispatcher();

    const model = getContext<Readable<DataModel>>("model");
    const diagram = getContext<Diagram>("diagram");
    const appearanceOptions =
        getContext<Writable<Options.AppearanceOptions>>("appearanceOptions");
    const dataOptions =
        getContext<Writable<Options.DataOptions>>("dataOptions");
    const layoutOptions =
        getContext<Writable<Options.LayoutOptions>>("layoutOptions");
    const exportOptions =
        getContext<Writable<Options.ExportOptions>>("exportOptions");
    const toolOptions =
        getContext<Writable<Options.ToolOptions>>("toolOptions");

    let importStateInput: HTMLInputElement | null = null;

    interface HelvegSerializedState {
        options: Options.ExplorerOptions;
        positions: Record<string, { x: number; y: number }>;
    }

    async function resetOptions() {
        dataOptions.set({...structuredClone(Options.DEFAULT_DATA_OPTIONS), ...diagram.options.refresh});
        exportOptions.set(structuredClone(Options.DEFAULT_EXPORT_OPTIONS));
        appearanceOptions.set(structuredClone(Options.DEFAULT_APPEARANCE_OPTIONS));
        layoutOptions.set(structuredClone(Options.DEFAULT_LAYOUT_OPTIONS));
        toolOptions.set(structuredClone(Options.DEFAULT_TOOL_OPTIONS));
        await diagram.reset();
    }

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

            let state = JSON.parse(result) as HelvegSerializedState;
            if (state) {
                dataOptions.set(state.options.data);
                exportOptions.set(state.options.export);
                appearanceOptions.set(state.options.appearance);
                layoutOptions.set(state.options.layout);
                toolOptions.set(state.options.tool);
                diagram.importPositions(state.positions);
            }
        };
    }

    function exportState() {
        const state: HelvegSerializedState = {
            options: {
                appearance: $appearanceOptions,
                data: $dataOptions,
                export: $exportOptions,
                layout: $layoutOptions,
                tool: $toolOptions,
            },
            positions: diagram.exportPositions(),
        };

        saveAs(
            new Blob([JSON.stringify(state)], {
                type: "application/json;charset=utf-8",
            }),
            `${$model.name}.${new Date()
                .toISOString()
                .slice(0, 10)}.helveg-state.json`
        );
    }
</script>

<Panel name="Document" indent={false} id={AppPanels.Document}>
    <Subpanel name="Theme">
        <button
            class="button-stretch primary"
            on:click|preventDefault={() =>
                window.document
                    .getElementsByClassName("explorer")[0]
                    .classList.toggle("dark")}
        >
            Toggle theme
        </button>
    </Subpanel>
    <Subpanel name="Metadata">
        <KeyValueList items={metadataItems} />
    </Subpanel>
    <Subpanel name="State">
        <input
            type="file"
            bind:this={importStateInput}
            style="display: none"
            on:change={(e) => importState(e.target)}
        />
        <div class="flex flex-col gap-4">
            <button
                class="button-stretch primary"
                on:click|preventDefault={resetOptions}
            >
                Reset options
            </button>
            <button
                class="button-stretch primary"
                on:click|preventDefault={() => importStateInput?.click()}
            >
                Import state
            </button>
            <button
                class="button-stretch primary"
                on:click|preventDefault={() => exportState()}
            >
                Export state
            </button>
        </div>
    </Subpanel>
    <Subpanel name="Export image">
        <label>
            <input type="checkbox" bind:checked={$exportOptions.includeNodes} />
            Nodes
        </label>

        <label>
            <input type="checkbox" bind:checked={$exportOptions.includeEdges} />
            Edges
        </label>

        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeLabels}
            />
            Labels
        </label>

        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeEffects}
            />
            Effects
        </label>

        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includePizzaDough}
            />
            Pizza dough
        </label>

        <label>
            <input
                type="checkbox"
                bind:checked={$exportOptions.includeHighlights}
            />
            Highlights
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            Scale
            <input type="number" min="1" bind:value={$exportOptions.scale} />
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            Background color
            <input type="color" bind:value={$exportOptions.backgroundColor} />
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            Opacity
            <input
                type="range"
                id="alpha"
                min="0"
                max="1"
                step="0.1"
                bind:value={$exportOptions.opacity}
            />
        </label>

        <button
            class="button-stretch primary"
            on:click|preventDefault={() => dispatch("export", $exportOptions)}
        >
            Export
        </button>
    </Subpanel>
</Panel>
