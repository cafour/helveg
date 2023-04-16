<script lang="ts">
    import type { GlyphOptions } from "model/options";
    import Panel from "./Panel.svelte";

    export let options: GlyphOptions;
</script>

<Panel name="Glyphs">
    <label>
        <input type="checkbox" bind:checked={options.showIcons} />
        ShowIcons
    </label>
    <label>
        <input type="checkbox" bind:checked={options.showOutlines} />
        ShowOutlines
    </label>
</Panel>
