<script lang="ts">
    import { type IconOptions, type Icon, type Diagram} from "../deps/helveg-diagram.ts";
    import { readable, type Readable } from "svelte/store";
    import { getContext } from "svelte";
    export let name: string;
    export let title: string | null = null;
    export let theme: string | null = null;
    
    let icons = getContext<Diagram>("diagram").options.iconRegistry;

    function getIconReadable(
        name: string,
        options?: IconOptions
    ): Readable<Icon> {
        return readable(icons.get(name, options), set => {
            let update = () => set(icons.get(name, options));
            icons.setAdded.subscribe(update);
            return () => icons.setAdded.unsubscribe(update);
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
    {#if $icon.format === "svg"}
        {@html $icon.data}
    {:else if $icon.format == "png"}
        <img src="data:image/png;base64,{$icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
