<script lang="ts" context="module">
    import { setContext, getContext, onMount } from "svelte";
    import { derived, writable, type Writable } from "svelte/store";

    export interface TabDescriptor {
        name: string;
        value: string;
    }

    export const tabDescriptorsKey = Symbol("tabDescriptors");
    export const currentTabKey = Symbol("currentTab");

    export class DockContext {
        static get tabDescriptors() {
            return getContext<Writable<TabDescriptor[]>>(tabDescriptorsKey);
        }

        static set tabDescriptors(value: Writable<TabDescriptor[]>) {
            setContext(tabDescriptorsKey, value);
        }

        static get currentTab() {
            return getContext<Writable<string | null>>(currentTabKey);
        }

        static set currentTab(value: Writable<string | null>) {
            setContext(currentTabKey, value);
        }
    }
</script>

<script lang="ts">
    DockContext.currentTab = writable<string | null>(null);
    DockContext.tabDescriptors = writable<TabDescriptor[]>([]);

    let currentTab = DockContext.currentTab;
    let tabDescriptors = DockContext.tabDescriptors;
</script>

<div class="dock">
    <ul class="tab-list">
        {#each $tabDescriptors as tabDescriptor}
            <li
                class="tab-item"
                on:click={() => $currentTab = tabDescriptor.value}
                on:keypress
            >
                {tabDescriptor.name}
            </li>
        {/each}
    </ul>

    <div class="tab-content">
        <slot />
    </div>
</div>
