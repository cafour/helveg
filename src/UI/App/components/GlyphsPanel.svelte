<script lang="ts">
    import type { GlyphOptions } from "model/options";
    import Panel from "./Panel.svelte";

    export let glyphOptions: GlyphOptions;
</script>

<Panel name="Glyphs">
    <label>
        <input type="checkbox" bind:checked={glyphOptions.showIcons} />
        ShowIcons
    </label>
    <label>
        <input type="checkbox" bind:checked={glyphOptions.showOutlines} />
        ShowOutlines
    </label>
    <label>
        <input type="checkbox" bind:checked={glyphOptions.showLabels} />
        ShowLabels
    </label>
    <label>
        <input type="checkbox" bind:checked={glyphOptions.showFire} />
        ShowFire
    </label>
</Panel>
