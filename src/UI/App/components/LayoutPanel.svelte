<script lang="ts">
    import type { LayoutOptions } from "model/options";
    import Panel from "./Panel.svelte";
    import { StructuralStatus, type StructuralDiagramStats } from "model/structural";
    import { createEventDispatcher } from "svelte";
    import Icon from "./Icon.svelte";
    import Subpanel from "./Subpanel.svelte";
    import KeyValueList from "./KeyValueList.svelte";

    export let options: LayoutOptions;
    export let status: StructuralStatus;
    export let stats: StructuralDiagramStats;

    $: items = [
        { key: "Iterations", value: stats?.iterationCount.toString() },
        { key: "Speed", value: `${stats?.speed.toFixed(3)} iterations/s` },
    ];

    let dispatch = createEventDispatcher();
</script>

<Panel name="Layout" indent={false}>
    <Subpanel name="TidyTree">
        <div class="flex flex-row justify-content-center">
            <button
                on:click={() => dispatch("tidyTree")}
                class="button-icon"
            >
                <Icon name="base:Run" title="Run" />
            </button>
        </div>
    </Subpanel>
    
    <Subpanel name="ForceAtlas2" indent={false}>
        <div class="flex flex-row justify-content-center">
            <button
                on:click={() => dispatch("run", false)}
                disabled={status == StructuralStatus.Running}
                class="button-icon"
            >
                <Icon name="base:Run" title="Run" />
            </button>
            <button
                on:click={() => dispatch("run", true)}
                disabled={status == StructuralStatus.RunningInBackground}
                class="button-icon"
            >
                <Icon name="base:RunAll" title="Run in background" />
            </button>
            <button
                on:click={() => dispatch("stop")}
                disabled={status == StructuralStatus.Stopped}
                class="button-icon"
            >
                <Icon name="base:Stop" title="Stop" />
            </button>
        </div>
        <KeyValueList {items} class="indent" />
    </Subpanel>
</Panel>
