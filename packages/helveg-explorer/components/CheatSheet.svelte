<script lang="ts">
    import ButtonIcon from "./ButtonIcon.svelte";
    import Dock from "./Dock.svelte";
    import Icon from "./Icon.svelte";
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
<div class="cheatsheet {isOpen ? 'visible' : 'invisible'}" on:click|self={() => (isOpen = false)}>
    <Dock name="cheatsheet" class="cheatsheet-inner" direction="flex-col" tabListDirection="row" allowDeselect={false}>
        <div class="cheatsheet-header" slot="before-tab-list">
            <Icon name="vscode:question" />
            CheatSheet
        </div>
        <button
            on:click={() => (isOpen = false)}
            type="button"
            class="button-icon primary close-button"
            slot="after-tab-list"
        >
            ✕
        </button>
        <Tab name="Controls" value="controls">This will a list of controls.</Tab>
        <Tab name="Glyphs" value="glyphs">This will a look at glyphs.</Tab>
        <Tab name="Panels" value="panels">This will a look at panels.</Tab>
    </Dock>
</div>
