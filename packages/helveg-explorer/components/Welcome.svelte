<script lang="ts" context="module">
    function setVisible(value: boolean): void {
        localStorage.setItem("Welcome.visible", value === true ? "true" : "false");
    }

    function getVisible(): boolean {
        const stored = localStorage.getItem("Welcome.visible");
        return stored === "true" || stored === null;
    }

    export const WELCOME_VISIBLE = writable(getVisible());
    WELCOME_VISIBLE.subscribe((v) => {
        setVisible(v);
    });
</script>

<script lang="ts">
    import ButtonStretch from "./ButtonStretch.svelte";
    import { writable } from "svelte/store";
    import { TUTORIAL_INDEX, TUTORIAL_VISIBLE } from "./Tutorial.svelte";

    function startWithTutorial() {
        $WELCOME_VISIBLE = false;
        $TUTORIAL_INDEX = 0;
        $TUTORIAL_VISIBLE = true;
    }

    function startWithoutTutorial() {
        $WELCOME_VISIBLE = false;
        $TUTORIAL_VISIBLE = false;
    }
</script>

<div class="welcome {$WELCOME_VISIBLE ? '' : 'hidden'}">
    <div class="inner">
        <h1>Welcome to Helveg!</h1>

        <p>
            Helveg is an experimental visualization tool of C# codebases. This is its interactive front-end. To
            visualize another codebase use <a href="https://www.nuget.org/packages/helveg" target="_blank"
                >the command-line tool</a
            >.
        </p>

        <p>
            If this is your first time using Helveg, you may want to go through the interactive tutorial or at least
            look at the cheat sheet. To access the cheat sheet, click on the questionmark in the lower left corner of
            the screen.
        </p>

        <p></p>

        <div class="flex flex-row gap-16">
            <ButtonStretch class="primary" on:click={() => startWithTutorial()}>Start tutorial</ButtonStretch>
            <ButtonStretch class="primary" on:click={() => startWithoutTutorial()}>Start without tutorial</ButtonStretch
            >
        </div>
    </div>
</div>
