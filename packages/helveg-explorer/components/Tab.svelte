<script lang="ts">
    import { onMount } from "svelte";
    import { DockContext } from "./Dock.svelte";

    let tabDescriptors = DockContext.tabDescriptors;
    let currentTab = DockContext.currentTab;

    export let name: string;
    export let value: string;
    export let icon: string | null;

    $: isSelected = value === $currentTab;

    onMount(() => {
        $tabDescriptors = [...$tabDescriptors, {
            name: name,
            value: value,
            icon: icon
        }];
    });
</script>

<div class="tab" class:hidden={!isSelected}>
    <slot />
</div>
