<script lang="ts">
    import type { ToolOptions } from "model/options";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons } from "model/const";
    import type { VisualizationModel } from "model/visualization";

    export let model: VisualizationModel;
    export let toolOptions: ToolOptions;
    
    $: relations = model ? Object.keys(model.multigraph.relations).sort() : [];
</script>

<Panel name="Tools" indent={false}>
    <Subpanel name="Cut" icon={AppIcons.CutTool}>
        <label>
            <input
                type="checkbox"
                bind:checked={toolOptions.isCuttingTransitive}
            />
            IsCuttingTransitive
        </label>
        <label class="flex flex-row gap-8 align-items-center">
            CuttingRelation
            <select bind:value={toolOptions.cuttingRelation}>
                {#each relations as relation}
                    <option value={relation}>{relation}</option>
                {/each}
            </select>
        </label>
    </Subpanel>
</Panel>
