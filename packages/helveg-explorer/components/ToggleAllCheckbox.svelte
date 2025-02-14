<script lang="ts">
    export let selected: any[];
    export let all: any[];

    let additionalClass: string | null = null;
    export { additionalClass as class };
</script>

<input
    class={additionalClass}
    type="checkbox"
    indeterminate={selected.length > 0 && selected.length < all.length}
    on:click={() => selected = selected.length > 0 ? [] : all}
    on:change
    checked={selected.length > 0}
/>
