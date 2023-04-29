<script lang="ts">
    import { DefaultEntityKindIcons } from "model/csharpPlugin";
    import { dataOptions, model } from "./App.svelte";
    import Subpanel from "./Subpanel.svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_ICON_NAME } from "model/glyph";
    
    $: kinds = !$model?.isEmpty
        ? Object.values($model.multigraph.nodes)
            .map(v => v.properties.Kind)
            .filter((kind, i, array) => kind != null && array.indexOf(kind) === i)
            .sort()
        : [];
</script>

<Subpanel name="IncludedCSharpKinds">
    {#each kinds as kind}
        <label>
            <input
                type="checkbox"
                bind:group={$dataOptions.csharp.includedKinds}
                value={kind}
            />
            <Icon name={DefaultEntityKindIcons[kind] ?? FALLBACK_ICON_NAME} />
            {kind}
        </label>
    {/each}
</Subpanel>
