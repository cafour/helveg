<script lang="ts">
    import { CSharpGlyphSizingMode } from "model/csharpPlugin";
    import Subpanel from "helveg";
    import { glyphOptions } from "../../UI";

    let sizingModes = Object.values(CSharpGlyphSizingMode)

    let sizingMode = $glyphOptions.csharp?.sizingMode ?? CSharpGlyphSizingMode.Linear;
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
