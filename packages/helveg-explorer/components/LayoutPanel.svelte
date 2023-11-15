<script lang="ts">
    import Panel from "./Panel.svelte";
    import { DiagramStatus, type DiagramStats, getRelations, getNodeKinds } from "../deps/helveg-diagram.ts";
    import { createEventDispatcher, getContext } from "svelte";
    import Icon from "./Icon.svelte";
    import Subpanel from "./Subpanel.svelte";
    import KeyValueList from "./KeyValueList.svelte";
    import { AppPanels } from "../const.ts";
    import type { Readable, Writable } from "svelte/store";
    import type { DataModel } from "../deps/helveg-diagram.ts";
    import type { DataOptions, LayoutOptions } from "../options.ts";
    import ToggleAllCheckbox from "./ToggleAllCheckbox.svelte";

    export let status: DiagramStatus;
    export let stats: DiagramStats;

    $: items = [
        { key: "Iterations", value: stats?.iterationCount.toString() },
        { key: "Speed", value: `${stats?.speed.toFixed(3)} iterations/s` },
    ];

    $: relations = getRelations($model.data);

    let dispatch = createEventDispatcher();

    let model = getContext<Readable<DataModel>>("model");
    let layoutOptions = getContext<Writable<LayoutOptions>>("layoutOptions");
        
    let dataOptions = getContext<Writable<DataOptions>>("dataOptions");

    $: kinds = getNodeKinds($model.data);

</script>

<Panel name="Layout" indent={false} id={AppPanels.Layout}>
    <Subpanel>
        <button on:click={() => dispatch("refresh")} class="button-stretch primary">
            Refresh
        </button>
    </Subpanel>
    
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
            Relation
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
    
    <Subpanel name="Relations">
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>
            <ToggleAllCheckbox
                bind:selected={$dataOptions.selectedRelations}
                all={relations}
            />
            <span>all</span>
        </label>
        {#each relations as relation}
            <label>
                <input
                    type="checkbox"
                    bind:group={$dataOptions.selectedRelations}
                    value={relation}
                />
                <span>{relation}</span>
            </label>
        {/each}
    </Subpanel>
    <Subpanel name="Node Kinds">
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>
            <ToggleAllCheckbox
                bind:selected={$dataOptions.selectedKinds}
                all={kinds}
            />
            <span>all</span>
        </label>
        {#each kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={$dataOptions.selectedKinds}
                    value={kind}
                />
                <span>{kind}</span>
            </label>
        {/each}
    </Subpanel>
</Panel>
