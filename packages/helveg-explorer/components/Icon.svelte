<script lang="ts">
    import { type IconOptions, type Icon, type Diagram, FALLBACK_ICON } from "../deps/helveg-diagram.ts";
    import { readable, type Readable } from "svelte/store";
    import { getContext } from "svelte";
    export let name: string | undefined;
    export let title: string | undefined = undefined;
    export let theme: string | undefined = undefined;
    let className: string | undefined = "";
    export { className as class };
    export let color: string | undefined = undefined;

    let icons = getContext<Diagram>("diagram").options.iconRegistry;

    function getIconReadable(name: string, options?: IconOptions): Readable<Icon> {
        return readable(icons.get(name, options), (set) => {
            let update = () => set(icons.get(name, options));
            icons.setAdded.subscribe(update);
            return () => icons.setAdded.unsubscribe(update);
        });
    }

    $: themeClass = theme != null ? `theme-${theme}` : "";

    $: icon = getIconReadable(name ?? FALLBACK_ICON.name, {
        viewBoxOnly: true,
        removeTitle: title != null,
        viewBox: "0 0 16 16",
        fill: color,
    });

    export let element: HTMLElement | null = null;
</script>

<div class="icon {themeClass} {$icon.namespace} {className}" {title} bind:this={element}>
    {#if $icon.format === "svg"}
        {@html $icon.data}
    {:else if $icon.format == "png"}
        <img src="data:image/png;base64,{$icon.data}" alt="The '{name}' icon" />
    {/if}
</div>
