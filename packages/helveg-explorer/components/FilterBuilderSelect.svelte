<script lang="ts" context="module">
    export interface SelectItem {
        value: string;
        label: string;
        group: string;
    }
</script>

<script lang="ts">
    import Select from "svelte-select";

    export let items: SelectItem[];
    let groupBy = (i: SelectItem) => i.group;
    export let value: string | undefined = undefined;
    let className: string | undefined = undefined;
    export { className as class };

    let valueItem: Partial<SelectItem> | undefined = value ? { value: value } : undefined;
    const updateValue = (item?: Partial<SelectItem>) => (value = item?.value);
    const updateValueItem = (value?: string) => {
        valueItem = value ? items.filter(i => i.value === value)[0] : undefined;
    };
    $: updateValue(valueItem);
    $: updateValueItem(value);
    export let clearable: boolean | undefined = undefined;
</script>

<Select
    {items}
    {groupBy}
    class="filter-builder-select {className}"
    placeholder="Apply a custom filter"
    floatingConfig={{
        strategy: "fixed",
    }}
    bind:value={valueItem}
    on:focus
    on:change
    listAutoWidth={false}
    {clearable}
    --background="var(--color-surface-45)"
    --list-background="var(--color-surface-45)"
    --list-border-radius="0"
    --border-radius="0"
    --list-max-height="100vh"
    --padding="0 0.5rem"
    --height="2rem"
    --border="1px solid var(--color-surface-100)"
    --value-container-padding="0.5rem 0"
    --item-padding="0 0.5rem"
    --item-line-height="2"
    --font-size="0.75rem"
    --group-title-font-size="0.75rem"
    --group-title-font-weight="900"
    --group-title-text-transform="none"
    --group-title-color="black"
    --group-item-padding-left="2rem"
    --item-hover-bg="var(--color-surface-100)"
    --item-height="auto"
    --item-is-not-selectable-color="black"
/>
