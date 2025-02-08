<script lang="ts">
    import ButtonIcon from "./ButtonIcon.svelte";
    import Dock from "./Dock.svelte";
    import Tab from "./Tab.svelte";

    let isOpen = false;

    export let buttonStyle: string | undefined = undefined;
</script>

<ButtonIcon class="cheatsheet-button large" icon="vscode:question" on:click={() => (isOpen = !isOpen)} style={buttonStyle} />

<div class="cheatsheet flex flex-col {isOpen ? 'visible' : 'invisible'}">
    <Dock name="cheatsheet">
        <Tab name="Controls" value="controls">This will a list of controls.</Tab>
    </Dock>
</div>
