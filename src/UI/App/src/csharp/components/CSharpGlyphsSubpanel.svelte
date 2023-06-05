<script lang="ts">
    import { CSharpGlyphSizingMode } from "../model";
    import { Subpanel, type AppearanceOptions } from "helveg";
    import type { Writable } from "svelte/store";
    import { getContext } from "svelte";

    let appearanceOptions =
        getContext<Writable<AppearanceOptions>>("appearanceOptions");

    let sizingModes = Object.values(CSharpGlyphSizingMode);

    let sizingMode =
        $appearanceOptions.glyph.csharp?.sizingMode ??
        CSharpGlyphSizingMode.Linear;
    $: $appearanceOptions.glyph.csharp!.sizingMode = sizingMode;
</script>

<Subpanel name="CSharpGlyphs" class="pb-8">
    <label class="flex flex-row gap-8 align-items-center">
        SizingMode
        <select bind:value={sizingMode}>
            {#each sizingModes as mode}
                <option value={mode}>{mode}</option>
            {/each}
        </select>
    </label>
</Subpanel>
