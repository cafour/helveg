<script lang="ts">
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import type { DataOptions } from "model/options";

    export let dataOptions: DataOptions;
</script>

<Panel name="Data" indent={false}>
    <div class="indent">
        <em class="mb-16 block">This panel is currently non-functional.</em>
        <button on:click={() => {}} class="button-stretch">Refresh Diagram</button>
    </div>
    <Subpanel name="IncludedKinds">
        {#each dataOptions.kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={dataOptions.selectedKinds}
                    value={kind}
                />
                <Icon name={dataOptions.defaultIcons[kind] ?? dataOptions.fallbackIcon} />
                {kind}
            </label>
        {/each}
    </Subpanel>
</Panel>
