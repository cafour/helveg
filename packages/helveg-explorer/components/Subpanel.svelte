<script lang="ts">
    import Hint from "./Hint.svelte";
    import Icon from "./Icon.svelte";

    export let name: string | null = null;
    export let indent: boolean = true;
    let additionalClass: string = "";
    export { additionalClass as class };
    export let bodyClass: string = "";
    export let icon: string | null = null;
    export let hint: string | null = null!;
    export let isCollapsible: boolean = true;
    export let collapsed: boolean = false;
</script>

<div class="subpanel flex flex-col {additionalClass}">
    {#if name !== null}
        <button
            class="subpanel-header px-4 flex flex-row align-items-center gap-4"
            on:click={() => (collapsed = !collapsed)}
            title={collapsed ? "Expand" : "Collapse"}
            disabled={!isCollapsible}
            type="button"
        >
            {#if isCollapsible}
                <button class="flex flex-col justify-content-center">
                    <Icon
                        name={collapsed
                            ? "vscode:chevron-right"
                            : "vscode:chevron-down"}
                    />
                </button>
            {/if}
            {#if icon !== null}
                <Icon name={icon} title={name} />
            {/if}
            <strong>{name}</strong>
            {#if hint !== null}
                <Hint text={hint} />
            {/if}
        </button>
    {/if}
    <div
        class="subpanel-body flex flex-col {bodyClass}"
        class:indent
        class:hidden={collapsed}
    >
        <slot />
    </div>
</div>
