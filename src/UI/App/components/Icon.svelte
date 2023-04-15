<script lang="ts">
    import { getIcon, type Icon, IconFormat } from "model/icons";
    import { onMount } from "svelte";
    export let name: string;

    let icon: Icon | null = null;
    onMount(() => {
        icon = getIcon(name);
    });
</script>

<div class="icon">
    {#if icon?.format === IconFormat.Svg}
        {@html icon.data}
    {:else if icon?.format == IconFormat.Png}
        <img src="data:image/png;base64,{icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
