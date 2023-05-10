<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { glyphOptions } from "./App.svelte";
    import { AppPanels } from "model/const";
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
