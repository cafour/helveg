<script lang="ts">
    import { createEventDispatcher } from "svelte";
    import { DiagramStatus } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let status: DiagramStatus;
    export let stoppable: boolean = true;

    const dispatch = createEventDispatcher();
</script>

<div
    class="loading-screen flex flex-col"
    class:hidden={status !== DiagramStatus.RunningInBackground}
>
    <div class="w-32 h-32">
        <Icon name="Fallback" />
    </div>
    <div>Running in the background...</div>
    {#if stoppable}
        <button
            class="button primary mt-16 flex flex-row gap-4 align-items-center justify-content-center"
            on:click={() => dispatch("stop")}
        >
            <Icon name="vscode:debug-stop" class="w-16"/>
            Stop
        </button>
    {/if}
</div>
