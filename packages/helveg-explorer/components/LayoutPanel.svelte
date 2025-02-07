<script lang="ts">
    import Panel from "./Panel.svelte";
    import {
        Diagram,
        DiagramStatus,
        type DiagramStats,
        getRelations,
        getNodeKinds,
        CSHARP_RELATION_HINTS,
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
    import Hint from "./Hint.svelte";
    import NodeKindIcon from "./NodeKindIcon.svelte";
    import ButtonStretch from "./ButtonStretch.svelte";
    import { OP_REFRESH } from "../operations/op-layout.ts";
    import type { IExplorerState } from "../explorer-state.ts";
    import { getShortcutHint } from "../operations/executor.ts";

    export let status: DiagramStatus;
    export let stats: DiagramStats;

    $: items = [
        { key: "Iterations", value: stats?.iterationCount.toString() },
        { key: "Speed", value: `${stats?.speed.toFixed(3)} iterations/s` },
        { key: "Global traction", value: stats?.globalTraction.toFixed(3) },
        { key: "Global swinging", value: stats?.globalSwinging.toFixed(3) },
        { key: "Average traction", value: stats?.averageTraction.toFixed(3) },
        { key: "Average swinging", value: stats?.averageSwinging.toFixed(3) },
    ];

    $: relations = getRelations($model.data);

    let dispatch = createEventDispatcher();

    const state = getContext<IExplorerState>("state");
    let diagram = getContext<Diagram>("diagram");
    let model = getContext<Readable<DataModel>>("model");
    let layoutOptions = getContext<Writable<LayoutOptions>>("layoutOptions");
    let dataOptions = getContext<Writable<DataOptions>>("dataOptions");

    $: kinds = getNodeKinds($model.data).sort(
        (a, b) => diagram.options.nodeKindOrder.indexOf(a) - diagram.options.nodeKindOrder.indexOf(b),
    );
</script>

<Panel name="Layout" indent={false} id={AppPanels.Layout}>
    <Subpanel class="sticky top-0">
        <ButtonStretch
            class="primary mb-8 flex flex-row gap-4 align-items-center justify-content-center"
            on:click={async () => await state.operationExecutor.triggerManually(OP_REFRESH, undefined)}
            hint={OP_REFRESH.hint}
            icon={OP_REFRESH.icon}
            name={OP_REFRESH.name}
            shortcut={getShortcutHint(OP_REFRESH.shortcut)}
        >
            Refresh
        </ButtonStretch>
        <label>
            <div class="flex flex-row gap-8">
                <span> ExpandedDepth </span>
                <Hint
                    text="The initial visible depth of the diagram. Press the Refresh button to reset the diagram to this depth."
                />
                <input type="number" min="-1" bind:value={$dataOptions.expandedDepth} />
            </div>
        </label>
    </Subpanel>

    <Subpanel name="Relations" hint="Allows you to pick which relationships are visualized.">
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>
            <ToggleAllCheckbox bind:selected={$dataOptions.selectedRelations} all={relations} />
            <span>all</span>
        </label>
        {#each relations as relation}
            <label>
                <input type="checkbox" bind:group={$dataOptions.selectedRelations} value={relation} />
                <span>{relation}</span>
                {#if CSHARP_RELATION_HINTS[relation] != null}
                    <Hint text={CSHARP_RELATION_HINTS[relation]} />
                {/if}
            </label>
        {/each}
    </Subpanel>

    <Subpanel
        name="Node Kinds"
        hint="Allows you to include or exclude nodes based on their kind. The list is sorted hierarchically with the 'largest' nodes at the top."
    >
        <!-- svelte-ignore a11y-label-has-associated-control -->
        <label>
            <ToggleAllCheckbox bind:selected={$dataOptions.selectedKinds} all={kinds} />
            <span>all</span>
        </label>
        {#each kinds as kind}
            <label>
                <input type="checkbox" bind:group={$dataOptions.selectedKinds} value={kind} />
                <NodeKindIcon nodeKind={kind} />
                <span>{kind}</span>
            </label>
        {/each}
    </Subpanel>

    <Subpanel
        name="TidyTree"
        hint="An immediate algorithm that lays out the graph in a circular tree."
        collapsed={true}
    >
        <div class="flex flex-row justify-content-center mb-8">
            <button on:click={() => dispatch("tidyTree")} class="button-icon success-stroke">
                <Icon name="vscode:play" title="Run" />
            </button>
        </div>
        <label class="flex flex-row gap-8 align-items-center">
            Relation
            <Hint
                text="The relation that gives structure to the repository. Should be a tree. We recommend keeping it set to 'declares'."
            />
            <select bind:value={$layoutOptions.tidyTree.relation}>
                {#each relations as relation}
                    <option value={relation}>{relation}</option>
                {/each}
            </select>
        </label>
    </Subpanel>

    <Subpanel
        name="ForceAtlas2"
        indent={true}
        hint="A continous algorithm that simulates forces between the nodes."
        collapsed={true}
    >
        <div class="flex flex-row justify-content-center">
            <button
                on:click={() => dispatch("run", false)}
                disabled={status == DiagramStatus.Running}
                class="button-icon success-stroke"
            >
                <Icon name="vscode:play" title="Run" />
            </button>
            <button
                on:click={() => dispatch("run", true)}
                disabled={status == DiagramStatus.RunningInBackground}
                class="button-icon success-stroke"
            >
                <Icon name="vscode:run-all" title="Run in background" />
            </button>
            <button
                on:click={() => dispatch("stop")}
                disabled={status == DiagramStatus.Stopped}
                class="button-icon primary-stroke"
            >
                <Icon name="vscode:debug-stop" title="Stop" />
            </button>
        </div>

        <hr />

        <div class="flex flex-col">
            <label>
                <input type="checkbox" bind:checked={$layoutOptions.forceAtlas2.adjustSizes} />
                <span>Adjust sizes</span>
                <Hint text="Prevent nodes from overlapping." />
            </label>
            <label>
                <input type="checkbox" bind:checked={$layoutOptions.forceAtlas2.barnesHutOptimize} />
                <span>Barnes-Hut optimize</span>
                <Hint text="Divide the nodes into regions are use them to approximate the forces." />
            </label>
            <label>
                <input type="checkbox" bind:checked={$layoutOptions.forceAtlas2.strongGravityMode} />
                <span>Strong gravity mode</span>
                <Hint
                    text="When enabled, fravity is linear with the distance from the center of the space. When disabled, gravity is inverse quadratic."
                />
            </label>
            <label>
                <input type="checkbox" bind:checked={$layoutOptions.forceAtlas2.linLogMode} />
                <span>LinLog mode</span>
                <Hint text="Use Noack's LinLog mode." />
            </label>
            <label>
                <input type="checkbox" bind:checked={$layoutOptions.forceAtlas2.outboundAttractionDistribution} />
                <span>Outbound attraction distribution</span>
            </label>
            <label>
                <div class="flex flex-row gap-8 space-nowrap">
                    <span>Gravity</span>
                    <input type="number" min="0" step="0.05" bind:value={$layoutOptions.forceAtlas2.gravity} />
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
                <div class="flex flex-row gap-8 space-nowrap">
                    <span>Scaling ratio</span>
                    <input type="number" min="1" bind:value={$layoutOptions.forceAtlas2.scalingRatio} />
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
                <div class="flex flex-row gap-8 space-nowrap">
                    <span>Slow down</span>
                    <Hint
                        text="Prevents the node from oscillating between two positions at the cost of the algorithm converging slower."
                    />
                    <input type="number" min="1" bind:value={$layoutOptions.forceAtlas2.slowDown} />
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
                <div class="flex flex-row gap-8 space-nowrap">
                    <span>Barnes-Hut theta</span>
                    <input type="number" min="0" step="0.05" bind:value={$layoutOptions.forceAtlas2.barnesHutTheta} />
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
            <label>
                <div class="flex flex-row gap-8 space-nowrap">
                    <span>AutoStop average traction</span>
                    <Hint
                        text="When the average traction of a node drops below this value, the algorithm automatically stops. Set to -1 to disable AutoStop altogether."
                    />
                    <input
                        type="number"
                        min="-1.0"
                        step="0.1"
                        bind:value={$layoutOptions.forceAtlas2.autoStopAverageTraction}
                    />
                </div>
                <input
                    type="range"
                    class="w-100p"
                    min="-1"
                    max="10"
                    step="0.1"
                    bind:value={$layoutOptions.forceAtlas2.autoStopAverageTraction}
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
</Panel>
