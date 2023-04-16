<script lang="ts">
    import { getIcon, type Icon, IconFormat, getIconReadable } from "model/icons";
    import { onMount } from "svelte";
    export let name: string;
    export let title: string | null = null;

    $: icon = getIconReadable(name, {
        viewBoxOnly: true,
        removeTitle: title != null,
        viewBox: "0 0 16 16"
    });
</script>

<div class="icon" {title}>
    {#if $icon.format === IconFormat.Svg}
        {@html $icon.data}
    {:else if $icon.format == IconFormat.Png}
        <img src="data:image/png;base64,{$icon.data}"
            alt="The '{name}' icon" />
    {/if}
</div>
