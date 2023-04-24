<script lang="ts">
    import { type IconOptions, type Icon, IconFormat } from "model/icons";
    import type { HelvegInstance } from "model/instance";
    import { readable, type Readable } from "svelte/store";
    import { getContext } from "svelte";
    export let name: string;
    export let title: string | null = null;
    
    let helveg = getContext<HelvegInstance>("helveg");

    function getIconReadable(
        name: string,
        options?: IconOptions
    ): Readable<Icon> {
        return readable(helveg.icons.get(name, options), set => {
            let update = () => set(helveg.icons.get(name, options));
            helveg.icons.setAdded.subscribe(update);
            return () => helveg.icons.setAdded.unsubscribe(update);
        });
    }

    $: icon = getIconReadable(name, {
        viewBoxOnly: true,
        removeTitle: title != null,
        viewBox: "0 0 16 16",
    });
</script>

<div class="icon" {title}>
    {#if $icon.format === IconFormat.Svg}
        {@html $icon.data}
    {:else if $icon.format == IconFormat.Png}
        <img src="data:image/png;base64,{$icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
