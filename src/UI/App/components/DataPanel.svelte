<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import type { DataOptions } from "model/options";

    export let options: DataOptions;
</script>

<Panel name="Data" indent={false}>
    <div class="indent">
        <button on:click={() => {}} class="w-100p">Refresh Diagram</button>
    </div>
    <Subpanel name="IncludedKinds">
        {#each options.kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={options.selectedKinds}
                    value={kind}
                />
                {kind}
            </label>
        {/each}
    </Subpanel>
</Panel>
