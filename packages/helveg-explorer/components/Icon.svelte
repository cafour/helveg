<script lang="ts">
    import type { IconOptions, Icon} from "../deps/helveg-diagram.ts";
    import { readable, type Readable } from "svelte/store";
    import { getContext } from "svelte";
    export let name: string;
    export let title: string | null = null;
    export let theme: string | null = null;
    
    let instance = getContext<HelvegInstance>("helveg");

    function getIconReadable(
        name: string,
        options?: IconOptions
    ): Readable<Icon> {
        return readable(instance.icons.get(name, options), set => {
            let update = () => set(instance.icons.get(name, options));
            instance.icons.setAdded.subscribe(update);
            return () => instance.icons.setAdded.unsubscribe(update);
        });
    }

    $: themeClass = theme != null ? `theme-${theme}` : "";
    
    $: icon = getIconReadable(name, {
        viewBoxOnly: true,
        removeTitle: title != null,
        viewBox: "0 0 16 16",
    });
</script>

<div class="icon {themeClass}" {title}>
    {#if $icon.format === window.helveg.IconFormat.Svg}
        {@html $icon.data}
    {:else if $icon.format == window.helveg.IconFormat.Png}
        <img src="data:image/png;base64,{$icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
