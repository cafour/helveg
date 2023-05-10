<script lang="ts">
    import { DefaultEntityKindIcons } from "model/csharpPlugin";
    import { dataOptions, model } from "./App.svelte";
    import Subpanel from "./Subpanel.svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_NODE_ICON } from "model/style";
    import ToggleAllCheckbox from "./ToggleAllCheckbox.svelte";

    $: kinds = !$model?.isEmpty
        ? Object.values($model.multigraph.nodes)
              .map((v) => v.properties.Kind)
              .filter(
                  (kind, i, array) => kind != null && array.indexOf(kind) === i
              )
              .sort()
        : [];

    let includedKinds = $dataOptions.csharp?.includedKinds ?? [];
    $: $dataOptions.csharp!.includedKinds = includedKinds;

    let autoExpandedKinds = $dataOptions.csharp?.autoExpandedKinds ?? [];
    $: $dataOptions.csharp!.autoExpandedKinds = autoExpandedKinds;
</script>

<Subpanel name="CSharpKinds" indent={false} class="pb-8">
    <div class="flex flex-col">
        <div class="flex flex-row gap-4 border-b-1 border-surface-100 justify-content-betweeen flex-grow-1 pb-8">
            <strong class=flex-grow-1>&nbsp</strong>
            <strong class="w-48 text-xs leading-100">Include</strong>
            <strong class="w-48 text-xs leading-100">Auto-expand</strong>
        </div>
        <div class="flex flex-row flex-grow-1 gap-4 pl-8 pr-8 align-items-center">
            <div class="space-nowrap flex-grow-1 ellipsis leading-150">
                All
            </div>
            <ToggleAllCheckbox
                class="w-48 h-16"
                bind:selected={includedKinds}
                all={kinds}
            />
            <ToggleAllCheckbox
                class="w-48 h-16"
                bind:selected={autoExpandedKinds}
                all={kinds}
            />
        </div>
        {#each kinds as kind}
            <div class="flex flex-row flex-grow-1 gap-4 pl-8 pr-8 align-items-center">
                <div class="space-nowrap flex-grow-1 ellipsis">
                    <Icon
                        name={DefaultEntityKindIcons[kind] ??
                            FALLBACK_NODE_ICON}
                    />
                    <span>{kind}</span>
                </div>
                <input
                    class="w-48 h-16"
                    type="checkbox"
                    bind:group={includedKinds}
                    value={kind}
                />
                <input
                    class="w-48 h-16"
                    type="checkbox"
                    bind:group={autoExpandedKinds}
                    value={kind}
                />
            </div>
        {/each}
    </div>
</Subpanel>
