<script lang="ts">
    import Icon from "./Icon.svelte";

    export let name: string | null = null;
    export let indent: boolean = true;
    let additionalClass: string = "";
    export { additionalClass as class };
    export let icon: string | null = null;
</script>

<div class="subpanel flex flex-col {additionalClass}">
    {#if name !== null}
        <div class="subpanel-header">
            {#if icon !== null}
                <Icon name={icon} title={name} />
            {/if}
            <strong>{name}</strong>
        </div>
    {/if}
    <div class="subpanel-body flex flex-col" class:indent>
        <slot />
    </div>
</div>
