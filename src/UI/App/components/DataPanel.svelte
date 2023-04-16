<script lang="ts">
    import Subpanel from "./Subpanel.svelte";
    import type { DataOptions } from "model/options";

    export let options: DataOptions;
</script>

<div class="panel flex flex-col overflow-hidden">
    <Subpanel name="IncludedKinds">
        {#each options.kinds as kind}
            <label>
                {kind}
                <input type="checkbox" bind:group={options.selectedKinds} value={kind} />
            </label>
        {/each}
    </Subpanel>
</div>
