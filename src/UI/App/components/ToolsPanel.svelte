<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "model/const";
    import { model, toolOptions } from "./App.svelte";

    $: relations = $model ? Object.keys($model.multigraph.relations).sort() : [];
</script>

<Panel name="Tools" indent={false} id={AppPanels.Tools}>
    <Subpanel name="ShowProperties" icon={AppIcons.ShowPropertiesTool}>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.showProperties.shouldHighlightSubtree}
            />
            ShouldHighlightSubtree
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.showProperties.shouldHighlightNeighbors}
            />
            ShouldHighlightNeighbors
        </label>
    </Subpanel>
    <Subpanel name="Toggle" icon={AppIcons.ToggleTool}>
        <label class="flex flex-row gap-8 align-items-center">
            Relation
            <select bind:value={$toolOptions.toggle.relation}>
                {#each relations as relation}
                    <option value={relation}>{relation}</option>
                {/each}
            </select>
        </label>
    </Subpanel>
    <Subpanel name="Cut" icon={AppIcons.CutTool}>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.cut.isTransitive}
            />
            IsTransitive
        </label>
        <label class="flex flex-row gap-8 align-items-center">
            Relation
            <select bind:value={$toolOptions.cut.relation}>
                {#each relations as relation}
                    <option value={relation}>{relation}</option>
                {/each}
            </select>
        </label>
    </Subpanel>
</Panel>
