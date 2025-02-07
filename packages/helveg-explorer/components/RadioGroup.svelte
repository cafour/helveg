<script lang="ts" context="module">
    export interface RadioItem {
        value: string | number;
        id?: string;
        label?: string;
        icon?: string;
        hideLabel?: boolean;
        hint?: string;
    }
</script>

<script lang="ts">
    import Icon from "./Icon.svelte";
    import Tooltip from "./Tooltip.svelte";

    export let items: RadioItem[];
    $: {
        for (const item of items) {
            item.id ??= `${groupName}-${item.value}`;
        }
    }
    export let selected = items[0]?.value;
    let additionalClass: string = "";
    export { additionalClass as class };
    export let groupName = Math.floor(Math.random() * 100).toString();
    let labels: Record<string, HTMLElement> = {};
</script>

<div class="radio-group {additionalClass}">
    {#each items as item}
        <input type="radio" name={groupName} bind:group={selected} on:change value={item.value} id={item.id} />
        <label for={item.id} bind:this={labels[item.id ?? ""]}>
            {#if item.icon}
                <Icon name={item.icon} title={item.label} />
            {/if}
            {#if item.label && !item.hideLabel}
                {item.label}
            {/if}
        </label>
        {#if item.hint != null}
            <Tooltip text={item.hint} target={labels[item.id ?? ""]} delay={500} />
        {/if}
    {/each}
</div>
