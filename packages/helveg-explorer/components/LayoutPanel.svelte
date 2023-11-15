<script lang="ts">
    import Panel from "./Panel.svelte";
    import {
        DiagramStatus,
        type DiagramStats,
        getRelations,
        getNodeKinds,
    } from "../deps/helveg-diagram.ts";
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
        <button
            on:click={() => dispatch("refresh")}
            class="button-stretch primary"
        >
            Refresh
        </button>
    </Subpanel>

    <Subpanel name="TidyTree">
        <div class="flex flex-row justify-content-center">
            <button on:click={() => dispatch("tidyTree")} class="button-icon">
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

    <Subpanel name="ForceAtlas2" indent={true}>
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

        <hr />

        <div class="flex flex-col">
            <label>
                <input
                    type="checkbox"
                    bind:checked={$layoutOptions.forceAtlas2.adjustSizes}
                />
                <span>AdjustSizes</span>
            </label>
            <label>
                <input
                    type="checkbox"
                    bind:checked={$layoutOptions.forceAtlas2.barnesHutOptimize}
                />
                <span>BarnesHutOptimize</span>
            </label>
            <label>
                <input
                    type="checkbox"
                    bind:checked={$layoutOptions.forceAtlas2.strongGravityMode}
                />
                <span>StrongGravityMode</span>
            </label>
            <label>
                <input
                    type="checkbox"
                    bind:checked={$layoutOptions.forceAtlas2.linLogMode}
                />
                <span>LinLogMode</span>
            </label>
            <label>
                <input
                    type="checkbox"
                    bind:checked={$layoutOptions.forceAtlas2.outboundAttractionDistribution}
                />
                <span>OutboundAttractionDistribution</span>
            </label>
            <label>
                <div class="flex flex-row gap-8">
                    <span>Gravity</span>
                    <input
                        type="number"
                        min="0"
                        step="0.05"
                        bind:value={$layoutOptions.forceAtlas2.gravity}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="0"
                    max="0.5"
                    step="0.05"
                    bind:value={$layoutOptions.forceAtlas2.gravity}
                />
            </label>
            <label>
                <div class="flex flex-row gap-8">
                    <span>ScalingRatio</span>
                    <input
                        type="number"
                        min="0"
                        bind:value={$layoutOptions.forceAtlas2.scalingRatio}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="1"
                    max="20"
                    step="1"
                    bind:value={$layoutOptions.forceAtlas2.scalingRatio}
                />
            </label>
            <label>
                <div class="flex flex-row gap-8">
                    <span>SlowDown</span>
                    <input
                        type="number"
                        min="0"
                        bind:value={$layoutOptions.forceAtlas2.slowDown}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="1"
                    max="20"
                    step="1"
                    bind:value={$layoutOptions.forceAtlas2.slowDown}
                />
            </label>
            <label>
                <div class="flex flex-row gap-8">
                    <span>BarnesHutTheta</span>
                    <input
                        type="number"
                        min="0"
                        step="0.05"
                        bind:value={$layoutOptions.forceAtlas2.barnesHutTheta}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="0"
                    max="5"
                    step="0.05"
                    bind:value={$layoutOptions.forceAtlas2.barnesHutTheta}
                />
            </label>
            <!-- <label>
                <div class="flex flex-row gap-8">
                    <span>EdgeWeightInfluence</span>
                    <input
                        type="number"
                        min="0"
                        step="0.1"
                        bind:value={$layoutOptions.forceAtlas2.edgeWeightInfluence}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="0"
                    max="10"
                    step="0.1"
                    bind:value={$layoutOptions.forceAtlas2.edgeWeightInfluence}
                />
            </label> -->
        </div>

        <hr />

        <KeyValueList {items} />
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
