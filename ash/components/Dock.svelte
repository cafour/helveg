<script lang="ts" context="module">
    import { setContext, getContext, onMount } from "svelte";
    import { writable, type Writable } from "svelte/store";

    export interface TabDescriptor {
        name: string;
        value: string;
        icon: string | null;
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
    import Icon from "./Icon.svelte";

    export let name: string;
    export let fallbackTab: string | null = null;

    DockContext.currentTab = writable<string | null>(null);
    DockContext.tabDescriptors = writable<TabDescriptor[]>([]);

    let currentTab = DockContext.currentTab;
    let tabDescriptors = DockContext.tabDescriptors;

    onMount(() => {
        const storedTab = localStorage.getItem(`Dock.${name}.currentTab`);
        setTab(storedTab && storedTab.length ? storedTab : fallbackTab);
    });

    export function setTab(value: string | null) {
        if (value === $currentTab) {
            return;
        }

        $currentTab = $currentTab === value ? null : value;
        localStorage.setItem(`Dock.${name}.currentTab`, $currentTab!);
    }

    export function toggleTab(value: string) {
        $currentTab = $currentTab === value ? null : value;
        localStorage.setItem(`Dock.${name}.currentTab`, $currentTab!);
    }

    let className: string | undefined;
    export { className as class };
</script>

<div class="dock flex flex-row-reverse relative {className}">
    <div class="tab-list flex flex-col">
        {#each $tabDescriptors as tabDescriptor}
            <!-- svelte-ignore a11y-no-static-element-interactions -->
            <div
                class="tab-item"
                class:active={$currentTab === tabDescriptor.value}
                on:click={() => toggleTab(tabDescriptor.value)}
                on:keypress
                title={tabDescriptor.name}
            >
                {#if tabDescriptor.icon}
                    <Icon
                        name={tabDescriptor.icon}
                        title={tabDescriptor.name}
                        theme="helveg"
                    />
                {/if}
            </div>
        {/each}
    </div>

    <div class="tab-content">
        <slot />
    </div>
</div>
