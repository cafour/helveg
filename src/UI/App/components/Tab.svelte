<script lang="ts">
    import { onMount } from "svelte";
    import { DockContext } from "./Dock.svelte";

    let tabDescriptors = DockContext.tabDescriptors;
    let currentTab = DockContext.currentTab;

    export let name: string;
    export let value: string;

    $: isSelected = value === $currentTab;

    onMount(() => {
        $tabDescriptors = [...$tabDescriptors, {
            name: name,
            value: value
        }];
    });
</script>

<div class="tab" class:hidden={!isSelected}>
    <slot />
</div>
