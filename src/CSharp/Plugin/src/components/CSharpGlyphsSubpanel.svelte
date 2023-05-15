<script lang="ts">
    import { CSharpGlyphSizingMode } from "model";
    import { Subpanel, type GlyphOptions } from "helveg";
    import type { Writable } from "svelte/store";
    import { getContext } from "svelte";

    let glyphOptions = getContext<Writable<GlyphOptions>>("glyphOptions");

    let sizingModes = Object.values(CSharpGlyphSizingMode);

    let sizingMode =
        $glyphOptions.csharp?.sizingMode ?? CSharpGlyphSizingMode.Linear;
    $: $glyphOptions.csharp!.sizingMode = sizingMode;
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
