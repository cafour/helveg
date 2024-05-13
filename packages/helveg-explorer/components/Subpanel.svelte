<script lang="ts">
    import Hint from "./Hint.svelte";
    import Icon from "./Icon.svelte";

    export let name: string | null = null;
    export let indent: boolean = true;
    let additionalClass: string = "";
    export { additionalClass as class };
    export let icon: string | null = null;
    export let hint: string | null = null!;
</script>

<div class="subpanel flex flex-col {additionalClass}">
    {#if name !== null}
        <div class="subpanel-header">
            {#if icon !== null}
                <Icon name={icon} title={name} />
            {/if}
            <strong>{name}</strong>
            {#if hint !== null}
                <Hint text={hint} />
            {/if}
        </div>
    {/if}
    <div class="subpanel-body flex flex-col" class:indent>
        <slot />
    </div>
</div>
