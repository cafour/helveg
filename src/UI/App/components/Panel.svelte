<script lang="ts">
    export let name: string | null = null;
    export let indent: boolean = true;
</script>

<div class="panel flex flex-col overflow-hidden">
    {#if name}
        <div class="panel-header">
            <h3>{name}</h3>
        </div>
    {/if}
    <div class="panel-body flex flex-col overflow-auto" class:indent>
        <slot />
    </div>
</div>
