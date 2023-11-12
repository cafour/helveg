<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppPanels } from "../const.ts";
    import { getContext } from "svelte";
    import type { Writable } from "svelte/store";
    import type { AppearanceOptions } from "../options.ts";

    let appearanceOptions = getContext<Writable<AppearanceOptions>>("appearanceOptions");
</script>

<Panel name="Appearance" indent={false} id={AppPanels.Appearance}>
    <Subpanel name="Glyphs">
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showIcons} />
            ShowIcons
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showOutlines} />
            ShowOutlines
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showLabels} />
            ShowLabels
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showFire} />
            ShowFire
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.isFireAnimated} />
            IsFireAnimated
        </label>
    </Subpanel>
    <Subpanel name="CodePizza">
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.codePizza.isEnabled} />
            CodePizza
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            CrustWidth
            <input
                type="number"
                min="1"
                bind:value={$appearanceOptions.codePizza.crustWidth}
            />
        </label>
        
        <label class="flex flex-row gap-8 align-items-center">
            SauceWidth
            <input
                type="number"
                min="1"
                bind:value={$appearanceOptions.codePizza.sauceWidth}
            />
        </label>
    </Subpanel>
</Panel>
