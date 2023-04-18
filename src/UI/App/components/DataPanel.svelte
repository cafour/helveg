<script lang="ts">
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import type { DataOptions } from "model/options";

    export let options: DataOptions;
</script>

<Panel name="Data" indent={false}>
    <div class="indent">
        <em class="mb-16 block">This panel is currently non-functional.</em>
        <button on:click={() => {}} class="button-stretch">Refresh Diagram</button>
    </div>
    <Subpanel name="IncludedKinds">
        {#each options.kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={options.selectedKinds}
                    value={kind}
                />
                <Icon name={options.defaultIcons[kind] ?? options.fallbackIcon} />
                {kind}
            </label>
        {/each}
    </Subpanel>
</Panel>
