<script lang="ts">
    import type { LayoutOptions } from "model/options";
    import Panel from "./Panel.svelte";
    import { StructuralStatus } from "model/structural";
    import { createEventDispatcher } from "svelte";
    import Icon from "./Icon.svelte";
    import Subpanel from "./Subpanel.svelte";

    export let options: LayoutOptions;
    export let status: StructuralStatus;
    
    let dispatch = createEventDispatcher();
</script>

<Panel name="Layout" indent={false}>
    <Subpanel name="ForceAtlas2">
        <button on:click={() => dispatch("run", false)} disabled={status != StructuralStatus.Stopped}>
            <Icon name="base:Run" title=Run/>
        </button>
        <button on:click={() => dispatch("run", true)} disabled={status != StructuralStatus.Stopped}>
            <Icon name="base:RunAll" title="Run in background" />
        </button>
        <button on:click={() => dispatch("stop")} disabled={status == StructuralStatus.Stopped}>
            <Icon name="base:Stop" title="Stop" />
        </button>
    </Subpanel>
</Panel>
