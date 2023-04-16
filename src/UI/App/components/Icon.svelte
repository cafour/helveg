<script lang="ts">
    import { getIcon, type Icon, IconFormat } from "model/icons";
    import { onMount } from "svelte";
    export let name: string;
    export let title: string | null = null;

    let icon: Icon | null = null;
    onMount(() => {
        icon = getIcon(name);
    });
</script>

<div class="icon" {title}>
    {#if icon?.format === IconFormat.Svg}
        {@html icon.data}
    {:else if icon?.format == IconFormat.Png}
        <img src="data:image/png;base64,{icon.data}"
            alt="The '{name}' icon" />
    {/if}
</div>
