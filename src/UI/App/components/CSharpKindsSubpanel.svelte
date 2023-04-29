<script lang="ts">
    import { DefaultEntityKindIcons } from "model/csharpPlugin";
    import { dataOptions, model } from "./App.svelte";
    import Subpanel from "./Subpanel.svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_ICON_NAME } from "model/glyph";

    $: kinds = !$model?.isEmpty
        ? Object.values($model.multigraph.nodes)
              .map((v) => v.properties.Kind)
              .filter(
                  (kind, i, array) => kind != null && array.indexOf(kind) === i
              )
              .sort()
        : [];
</script>

<Subpanel name="CSharpKinds" indent={false} class="pb-8">
    <div class="flex flex-col gap-8">
        <div class="flex flex-row gap-4 border-b-1 border-surface-100 justify-content-betweeen flex-grow-1">
            <strong class=flex-grow-1>&nbsp</strong>
            <strong>Included</strong>
            <strong>Auto-expanded</strong>
        </div>
        {#each kinds as kind}
            <div class="flex flex-row flex-grow-1 gap-4 pl-8 pr-8   ">
                <div class="space-nowrap flex-grow-1">
                    <Icon
                        name={DefaultEntityKindIcons[kind] ??
                            FALLBACK_ICON_NAME}
                    />
                    {kind}
                </div>
                <input
                    type="checkbox"
                    bind:group={$dataOptions.csharp.includedKinds}
                    value={kind}
                />
                <input
                    type="checkbox"
                    bind:group={$dataOptions.csharp.autoExpandedKinds}
                    value={kind}
                />
            </div>
        {/each}
    </div>
</Subpanel>
