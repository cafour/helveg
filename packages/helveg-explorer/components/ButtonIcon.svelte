<script lang="ts">
    import { FALLBACK_ICON } from "../deps/helveg-diagram.ts";
    import Icon from "./Icon.svelte";
    import Tooltip from "./Tooltip.svelte";

    let className: string | undefined = undefined;
    export { className as class };
    export let style: string | undefined = undefined;
    export let icon: string | undefined = undefined;
    export let name: string | undefined = undefined;
    export let hint: string | undefined = undefined;
    export let shortcut: string | undefined = undefined;
    export let disabled: boolean = false;

    let element: HTMLElement;
</script>

<button class="button-icon {className}" on:click bind:this={element} {style} {disabled}>
    <Icon name={icon ?? FALLBACK_ICON.name} />
</button>

{#if hint != null}
    <Tooltip bind:target={element} text={hint} delay={500} header={name} {shortcut} />
{/if}
