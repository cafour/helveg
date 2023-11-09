<script lang="ts">
    import type { AppPanels } from "model/const";
    import type { HelvegInstance } from "model/instance";
    import { getAllContexts, getContext, onMount } from "svelte";

    export let name: string | null = null;
    export let id: AppPanels;
    export let indent: boolean = true;

    let instance = getContext<HelvegInstance>("helveg");

    let subpanels: HTMLElement;

    let contexts = getAllContexts();
    
    onMount(() => {
        let extensions = instance.uiExtensions.for(id);
        extensions.forEach(ex => new ex.component({
            target: subpanels,
            context: contexts,
            hydrate: false
        }));
    });
</script>

<div class="panel flex flex-col overflow-hidden" {id}>
    {#if name}
        <div class="panel-header">
            <h3>{name}</h3>
        </div>
    {/if}
    <div
        class="panel-body flex flex-col overflow-auto"
        class:indent
        bind:this={subpanels}
    >
        <slot />
    </div>
</div>
