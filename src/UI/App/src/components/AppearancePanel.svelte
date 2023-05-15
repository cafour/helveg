<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppPanels } from "model/const";
    import { getContext } from "svelte";
    import type { Writable } from "svelte/store";
    import type { GlyphOptions } from "model/options";

    let glyphOptions = getContext<Writable<GlyphOptions>>("glyphOptions");
</script>

<Panel name="Appearance" indent={false} id={AppPanels.Appearance}>
    <Subpanel name="Glyphs">
        <label>
            <input type="checkbox" bind:checked={$glyphOptions.showIcons} />
            ShowIcons
        </label>
        <label>
            <input type="checkbox" bind:checked={$glyphOptions.showOutlines} />
            ShowOutlines
        </label>
        <label>
            <input type="checkbox" bind:checked={$glyphOptions.showLabels} />
            ShowLabels
        </label>
        <label>
            <input type="checkbox" bind:checked={$glyphOptions.showFire} />
            ShowFire
        </label>
        <label>
            <input type="checkbox" bind:checked={$glyphOptions.isFireAnimated} />
            IsFireAnimated
        </label>
    </Subpanel>
</Panel>
