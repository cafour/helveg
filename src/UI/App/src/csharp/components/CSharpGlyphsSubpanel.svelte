<script lang="ts">
    import { CSharpGlyphSizingMode, DEFAULT_CSHARP_GLYPH_OPTIONS, IconableEntities } from "../model";
    import { Subpanel, type AppearanceOptions, PizzaIcons } from "helveg";
    import type { Writable } from "svelte/store";
    import { getContext } from "svelte";

    let appearanceOptions =
        getContext<Writable<AppearanceOptions>>("appearanceOptions");

    let sizingModes = Object.values(CSharpGlyphSizingMode);

    let sizingMode =
        $appearanceOptions.glyph.csharp?.sizingMode ??
        CSharpGlyphSizingMode.Linear;
    $: $appearanceOptions.glyph.csharp!.sizingMode = sizingMode;

    let pizzaEntities = Object.keys(IconableEntities).sort();
    let toppingOptions = Object.values(PizzaIcons).sort();

    let toppings = $appearanceOptions.glyph.csharp?.pizzaToppings
        ?? structuredClone(DEFAULT_CSHARP_GLYPH_OPTIONS.pizzaToppings);
    $: $appearanceOptions.glyph.csharp!.pizzaToppings = toppings;
</script>

<Subpanel name="C# Glyphs" class="pb-8">
    <label class="flex flex-row gap-8 align-items-center">
        SizingMode
        <select bind:value={sizingMode}>
            {#each sizingModes as mode}
                <option value={mode}>{mode}</option>
            {/each}
        </select>
    </label>
</Subpanel>

<Subpanel name="C# CodePizza Toppings" class="pb-8">
    {#each pizzaEntities as entity}
        <label class="flex flex-row gap-8 align-items-center">
            <span class="w-80 inline-block flex-shrink-0 ellipsis overflow-hidden" title={entity}>{entity}</span>
            <select bind:value={toppings[entity]}>
                {#each toppingOptions as topping}
                    <option value={topping}>{topping}</option>
                {/each}
            </select>
        </label>
    {/each}
</Subpanel>
