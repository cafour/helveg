<script lang="ts">
    import ButtonIcon from "./ButtonIcon.svelte";
    import Dock from "./Dock.svelte";
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

<div class="cheatsheet {isOpen ? 'visible' : 'invisible'}">
    <Dock
        name="cheatsheet"
        class="cheatsheet-inner"
        direction="flex-col"
        tabListDirection="row"
        allowDeselect={false}
    >
        <Tab name="Controls" value="controls">This will a list of controls.</Tab>
        <Tab name="Glyphs" value="glyphs">This will a look at glyphs.</Tab>
        <Tab name="Panels" value="panels">This will a look at panels.</Tab>
    </Dock>
</div>
