<script lang="ts">
    import { DiagramStatus } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";

    export let status: DiagramStatus;
</script>

<div
    class="loading-screen flex flex-col"
    class:hidden={status !== DiagramStatus.RunningInBackground}
>
    <div class="w-32 h-32">
        <Icon name="Fallback" />
    </div>
    <div>Running in the background...</div>
</div>
