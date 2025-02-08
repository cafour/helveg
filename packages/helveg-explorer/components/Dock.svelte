<script lang="ts" context="module">
    import { setContext, getContext, onMount } from "svelte";
    import { writable, type Writable } from "svelte/store";

    export interface TabDescriptor {
        name: string;
        value: string;
        icon?: string;
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

    export let allowDeselect: boolean = true;
    onMount(() => {
        const storedTab = localStorage.getItem(`Dock.${name}.currentTab`);
        setTab(storedTab && storedTab.length ? storedTab : fallbackTab);
    });

    
    export function setTab(value: string | null) {
        if (value === $currentTab) {
            return;
        }

        $currentTab = (allowDeselect && $currentTab === value) ? null : value;
        localStorage.setItem(`Dock.${name}.currentTab`, $currentTab!);
    }

    export function toggleTab(value: string) {
        $currentTab = (allowDeselect && $currentTab === value) ? null : value;
        localStorage.setItem(`Dock.${name}.currentTab`, $currentTab!);
    }

    let className: string | undefined = undefined;
    export { className as class };
    export let direction: string = "flex-row-reverse";
    export let tabListDirection: string = "col";
    export let style: string | undefined = undefined;
</script>

<div class="dock flex {direction} relative {className}" {style}>
    <div class="tab-list flex {tabListDirection}">
        {#each $tabDescriptors as tabDescriptor}
            <!-- svelte-ignore a11y-no-static-element-interactions -->
            <div
                class="tab-item"
                class:active={$currentTab === tabDescriptor.value}
                class:with-icon={tabDescriptor.icon != null}
                on:click={() => toggleTab(tabDescriptor.value)}
                on:keypress
                title={tabDescriptor.name}
            >
                {#if tabDescriptor.icon}
                    <Icon name={tabDescriptor.icon} title={tabDescriptor.name} theme="helveg" />
                {:else}
                    <span>{tabDescriptor.name}</span>
                {/if}
            </div>
        {/each}
    </div>

    <div class="tab-content">
        <slot />
    </div>
</div>
