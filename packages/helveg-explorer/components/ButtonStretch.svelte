<script lang="ts">
    import Icon from "./Icon.svelte";
    import Tooltip from "./Tooltip.svelte";

    let className: string | undefined;
    export { className as class };
    export let icon: string | undefined = undefined;
    export let name: string | undefined = undefined;
    export let hint: string | undefined = undefined;
    export let shortcut: string | undefined = undefined;

    let element: HTMLElement;
</script>

<button on:click bind:this={element} class="button-stretch {className}" type="button">
    {#if icon != null}
        <Icon name={icon} />
    {/if}
    <slot />
</button>
{#if hint != null}
    <Tooltip bind:target={element} text={hint} delay={500} header={name} {shortcut} />
{/if}
