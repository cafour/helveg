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
    <Subpanel name="Show Properties" icon={AppIcons.ShowPropertiesTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldHighlightSubtree} />
            Highlight subtree
            <Hint
                text="When inspecting a node, highlight all of its descendants."
            />
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldHighlightNeighbors} />
            Highlight neighbors
            <Hint
                text="When inspecting a node, highlight all of its neighboring nodes."
            />
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldFixateSelectedNode} />
            Fixate selected nodes
            <Hint
                text="Keep the currently inspected node in place -- let it be ignored by the layout algorithm."
            />
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.showProperties.shouldFocusPropertiesPanel} />
            Focus properties panel
            <Hint
                text="Open the Properties panel when you select a node."
            />
        </label>
    </Subpanel>
    <Subpanel name="Remove" icon={AppIcons.RemoveTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.remove.isTransitive} />
            Transitive
            <Hint
                text="Remove all descendants of the removed node."
            />
        </label>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.remove.shouldRunLayout} />
            Run layout
            <Hint
                text="Run the continuous layout algorithm when a node is deleted."
            />
        </label>
    </Subpanel>
    <Subpanel name="Toggle" icon={AppIcons.ToggleTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.toggle.shouldRunLayout} />
            Run layout
            <Hint
                text="Run the continuous layout algorithm after every node-expanding or node-collapsing operation."
            />
        </label>
    </Subpanel>
    <Subpanel name="Move" icon={AppIcons.MoveTool}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.move.shouldRunLayout} />
            Run layout
            <Hint
                text="Run the continuous layout algorithm when a node is manually moved."
            />
        </label>
    </Subpanel>
    <Subpanel name="Search" icon={AppIcons.SearchPanel}>
        <label>
            <input type="checkbox" bind:checked={$toolOptions.search.shouldRunLayout} />
            Run layout
            <Hint
                text="Run the continuous layout algorithm after a Highlight or Isolate operation."
            />
        </label>
    </Subpanel>
</Panel>
