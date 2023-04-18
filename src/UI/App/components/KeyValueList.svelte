<script lang="ts">
    export let items: {key: string, value: string | null}[] = [];
    let additionalClass: string = "";
    export {additionalClass as class};
</script>

<dl class="flex flex-col overflow-hidden {additionalClass}">
    {#each items as item}
    {#if item.value !== null}
        <div class="flex flex-row flex-wrap space-nowrap gap-1 justify-content-between">
            <dt class="bold border-r-1">{item.key}</dt>
            <dd class="flex-grow-1 text-right">{item.value}</dd>
        </div>
    {/if}
    {/each}
</dl>
