<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import type { Writable } from "svelte/store";
    import { getContext } from "svelte";
    import type { ToolOptions } from "../options.ts";
    import Hint from "./Hint.svelte";

    let toolOptions = getContext<Writable<ToolOptions>>("toolOptions");
</script>

<Panel name="Tools" indent={false} id={AppPanels.Tools}>
    <Subpanel name="ShowProperties" icon={AppIcons.ShowPropertiesTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldHighlightSubtree} />
            ShouldHighlightSubtree
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldHighlightNeighbors} />
            ShouldHighlightNeighbors
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldFixateSelectedNode} />
            ShouldFixateSelectedNodes
            <Hint
                text="Whether to keep the currently inspected node in place -- let it be ignored by the layout algorithm."
            />
        </label>
    </Subpanel>
    <Subpanel name="Remove" icon={AppIcons.RemoveTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.remove.isTransitive} />
            IsTransitive
        </label>
    </Subpanel>
    <Subpanel name="Toggle" icon={AppIcons.ToggleTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.toggle.shouldRunLayout} />
            ShouldRunLayout
            <Hint
                text="Whether to automatically run the continuous layout algorithm after every node-expanding or node-collapsing operation."
            />
        </label>
    </Subpanel>
</Panel>
