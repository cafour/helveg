<script lang="ts" context="module">
    export interface RadioItem {
        value: string | number;
        id?: string;
        label?: string;
        icon?: string;
    }
</script>

<script lang="ts">
    import Icon from "./Icon.svelte";

    export let items: RadioItem[];
    export let selected = items[0]?.value;
    let additionalClass: string = "";
    export { additionalClass as class };
    export let groupName = Math.floor(Math.random() * 100).toString();
</script>

<div class="radio-group {additionalClass}">
    {#each items as item}
        <input
            type="radio"
            name={groupName}
            bind:group={selected}
            value={item.value}
            id={item.id ?? `radio-${item.value}`}
        />
        <label for={item.id ?? `radio-${item.value}`}>
            {#if item.icon}
                <Icon name={item.icon} />
            {/if}
            {#if item.label}
                {item.label}
            {/if}
        </label>
    {/each}
</div>
