<script lang="ts">
    import AboutInfo from "./AboutInfo.svelte";
    import ButtonIcon from "./ButtonIcon.svelte";
    import ControlsInfo from "./ControlsInfo.svelte";
    import Dock from "./Dock.svelte";
    import GlyphInfo from "./GlyphInfo.svelte";
    import Icon from "./Icon.svelte";
    import PanelsInfo from "./PanelsInfo.svelte";
    import Tab from "./Tab.svelte";

    let isOpen = false;

    export let buttonStyle: string | undefined = undefined;
</script>

<ButtonIcon
    class="cheatsheet-button large"
    icon="vscode:question"
    on:click={() => (isOpen = !isOpen)}
    style={buttonStyle}
/>

<!-- svelte-ignore a11y-no-static-element-interactions -->
<!-- svelte-ignore a11y-click-events-have-key-events -->
<div class="cheatsheet {isOpen ? '' : 'hidden'}" on:click|self={() => (isOpen = false)}>
    <Dock
        name="cheatsheet"
        class="cheatsheet-inner"
        direction="flex-col"
        tabListDirection="row"
        allowDeselect={false}
        fallbackTab="controls"
    >
        <div class="cheatsheet-header" slot="before-tab-list">  
            CheatSheet
        </div>
        <div slot="after-tab-list" class="flex flex-row">
            <a
                href="https://helveg.net/user-guide/"
                class="button-stretch light-surface flex flex-row gap-4 align-items-center justify-content-center"
                target="_blank"
            >
                <Icon name="vscode:link-external" class="w-16" />
                Documentation
            </a>
            <button on:click={() => (isOpen = false)} type="button" class="button-icon primary close-button">
                ✕
            </button>
        </div>
        <Tab name="Controls" value="controls">
            <ControlsInfo />
        </Tab>
        <Tab name="Glyphs" value="glyphs">
            <GlyphInfo />
        </Tab>
        <Tab name="Panels" value="panels">
            <PanelsInfo />
        </Tab>
        <Tab name="About" value="about">
            <AboutInfo />
        </Tab>
    </Dock>
</div>
