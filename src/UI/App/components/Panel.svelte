<script lang="ts">
    import type { AppPanels } from "model/const";
    import type { HelvegInstance } from "model/instance";
    import { getContext } from "svelte";

    export let name: string | null = null;
    export let id: AppPanels;
    export let indent: boolean = true;
    
    let instance = getContext<HelvegInstance>("helveg");
    
    let extensions = instance.uiExtensions.for(id);
</script>

<div class="panel flex flex-col overflow-hidden" {id}>
    {#if name}
        <div class="panel-header">
            <h3>{name}</h3>
        </div>
    {/if}
    <div class="panel-body flex flex-col overflow-auto" class:indent>
        <slot />
        {#each extensions as extension}
            <svelte:component this={extension.component} />
        {/each}
    </div>
</div>
