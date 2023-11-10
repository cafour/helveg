<script lang="ts">
    import Panel from "./Panel.svelte";
    import { DiagramStatus, type DiagramStats } from "../deps/helveg-diagram.ts";
    import { createEventDispatcher, getContext } from "svelte";
    import Icon from "./Icon.svelte";
    import Subpanel from "./Subpanel.svelte";
    import KeyValueList from "./KeyValueList.svelte";
    import { AppPanels } from "../const.ts";
    import type { Readable, Writable } from "svelte/store";
    import type { VisualizationModel } from "../deps/helveg-diagram.ts";
    import type { LayoutOptions } from "../options.ts";

    export let status: DiagramStatus;
    export let stats: DiagramStats;

    $: items = [
        { key: "Iterations", value: stats?.iterationCount.toString() },
        { key: "Speed", value: `${stats?.speed.toFixed(3)} iterations/s` },
    ];

    $: relations = $model ? Object.keys($model.multigraph.relations).sort() : [];

    let dispatch = createEventDispatcher();

    let model = getContext<Readable<VisualizationModel>>("model");
    let layoutOptions = getContext<Writable<LayoutOptions>>("layoutOptions");

</script>

<Panel name="Layout" indent={false} id={AppPanels.Layout}>
    <Subpanel name="TidyTree">
        <div class="flex flex-row justify-content-center">
            <button
                on:click={() => dispatch("tidyTree")}
                class="button-icon"
            >
                <Icon name="vs:Run" title="Run" />
            </button>
        </div>
        <label class="flex flex-row gap-8 align-items-center">
            TidyTreeRelation
            <select bind:value={$layoutOptions.tidyTree.relation}>
                {#each relations as relation}
                    <option value={relation}>{relation}</option>
                {/each}
            </select>
        </label>
    </Subpanel>
    
    <Subpanel name="ForceAtlas2" indent={false}>
        <div class="flex flex-row justify-content-center">
            <button
                on:click={() => dispatch("run", false)}
                disabled={status == DiagramStatus.Running}
                class="button-icon"
            >
                <Icon name="vs:Run" title="Run" />
            </button>
            <button
                on:click={() => dispatch("run", true)}
                disabled={status == DiagramStatus.RunningInBackground}
                class="button-icon"
            >
                <Icon name="vs:RunAll" title="Run in background" />
            </button>
            <button
                on:click={() => dispatch("stop")}
                disabled={status == DiagramStatus.Stopped}
                class="button-icon"
            >
                <Icon name="vs:Stop" title="Stop" />
            </button>
        </div>
        <KeyValueList {items} class="indent" />
    </Subpanel>
</Panel>
