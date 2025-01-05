<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import type { Writable } from "svelte/store";
    import { getContext } from "svelte";
    import type { ToolOptions } from "../options.ts";

    let toolOptions = getContext<Writable<ToolOptions>>("toolOptions");
</script>

<Panel name="Tools" indent={false} id={AppPanels.Tools}>
    <Subpanel name="ShowProperties" icon={AppIcons.ShowPropertiesTool}>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.showProperties
                    .shouldHighlightSubtree}
            />
            ShouldHighlightSubtree
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.showProperties
                    .shouldHighlightNeighbors}
            />
            ShouldHighlightNeighbors
        </label>
    </Subpanel>
    <Subpanel name="Remove" icon={AppIcons.RemoveTool}>
        <label>
            <input
                type="checkbox"
                bind:checked={$toolOptions.remove.isTransitive}
            />
            IsTransitive
        </label>
    </Subpanel>
</Panel>
