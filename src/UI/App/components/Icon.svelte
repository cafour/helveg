<script lang="ts" context="module">
    import { type Icon, type IconSet, IconFormat } from "model/icons";
    import { onMount } from "svelte";

    export let iconSets: Record<string, IconSet> = {};

    let iconScripts = document.getElementsByClassName("helveg-icons");
    for (let iconScript of iconScripts) {
        let iconSet = <IconSet>JSON.parse(iconScript.textContent!);
        iconSets[iconSet.namespace] = iconSet;
    }
</script>

<script lang="ts">
    export let name: string;

    let icon: Icon | null = null;
    onMount(() => {
        let segments = name.split(":", 2);
        let namespace = segments[0];
        let iconName = segments[1];
        let iconSet = iconSets[namespace];
        if (!iconSet) {
            throw new Error(
                `Icon set for namespace '${namespace}' could not be found.`
            );
        }

        icon = iconSet.icons[iconName];
        if (!icon) {
            throw new Error(`Icon '${name}' could not be found.`);
        }
    });
</script>

<div class="icon">
    {#if icon?.format === IconFormat.Svg}
        {@html icon.data}
    {:else if icon?.format == IconFormat.Png}
        <img src="data:image/png;base64,{icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
