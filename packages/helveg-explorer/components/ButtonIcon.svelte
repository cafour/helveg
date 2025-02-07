<script lang="ts">
    import { FALLBACK_ICON } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";
    import Tooltip from "./Tooltip.svelte";

    let className: string | undefined;
    export { className as class };
    export let icon: string | undefined;
    export let name: string | undefined;
    export let hint: string | undefined;
    export let shortcut: string | undefined;

    let element: HTMLElement;
</script>

<button class="button-icon {className}" on:click bind:this={element}>
    <Icon name={icon ?? FALLBACK_ICON.name} />
</button>
{#if hint != null}
    <Tooltip bind:target={element} text={hint} delay={500} header={name} {shortcut} />
{/if}
