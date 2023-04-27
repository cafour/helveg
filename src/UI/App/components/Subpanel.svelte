<script lang="ts">
    export let name: string | null = null;
    export let indent: boolean = true;
    let additionalClass: string = "";
    export { additionalClass as class };
</script>

<div class="subpanel flex flex-col {additionalClass}">
    {#if name !== null}
        <div class="subpanel-header">
            <strong>{name}</strong>
        </div>
    {/if}
    <div class="subpanel-body flex flex-col" class:indent>
        <slot />
    </div>
</div>
