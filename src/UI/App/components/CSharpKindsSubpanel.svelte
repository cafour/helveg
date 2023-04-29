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
    <table>
        <thead>
            <tr>
                <th>&nbsp;</th>
                <th>Included</th>
                <th>Auto-expanded</th>
            </tr>
        </thead>
        <tbody>
            {#each kinds as kind}
                <tr>
                    <td class="space-nowrap">
                        <Icon
                            name={DefaultEntityKindIcons[kind] ??
                                FALLBACK_ICON_NAME}
                        />
                        {kind}
                    </td>
                    <td class="text-center">
                        <input
                            type="checkbox"
                            bind:group={$dataOptions.csharp.includedKinds}
                            value={kind}
                        />
                    </td>
                    <td class="text-center">
                        <input
                            type="checkbox"
                            bind:group={$dataOptions.csharp.autoExpandedKinds}
                            value={kind}
                        />
                    </td></tr
                >
            {/each}
        </tbody>
    </table>
</Subpanel>
