<script lang="ts" context="module">
    import { writable } from "svelte/store";
    function setVisible(value: boolean): void {
        localStorage.setItem("Tutorial.visible", value === true ? "true" : "false");
    }

    function getVisible(): boolean {
        const stored = localStorage.getItem("Tutorial.visible");
        return stored === "true";
    }

    export const TUTORIAL_VISIBLE = writable(getVisible());
    TUTORIAL_VISIBLE.subscribe((v) => {
        setVisible(v);
    });
</script>

<script lang="ts">
    import { TUTORIAL_MESSAGES } from "../tutorial.ts";
    import { setPopupPosition } from "../popups.ts";
    import { onMount } from "svelte";
    import ButtonIcon from "./ButtonIcon.svelte";

    const messages = TUTORIAL_MESSAGES;
    let currentIndex: number = 0;

    function setMessage(index: number) {
        if (index >= TUTORIAL_MESSAGES.length) {
            $TUTORIAL_VISIBLE = false;
            return;
        }

        currentIndex = index;
        const position = messages[currentIndex].position;
        if (position.elementQuery) {
            setPopupPosition(element, document.querySelector(position.elementQuery)!);
        } else {
            element.style.top = position.top ?? "";
            element.style.bottom = position.bottom ?? "";
            element.style.left = position.left ?? "";
            element.style.right = position.right ?? "";
            element.classList.remove("arrow-top");
            element.classList.remove("arrow-bottom");
        }
    }

    onMount(() => {
        setMessage(0);
    });

    let element: HTMLElement;
</script>

<div class="tutorial" bind:this={element} class:hidden={!$TUTORIAL_VISIBLE}>
    <div class="tutorial-header">
        {#if messages[currentIndex].header != null}
            <strong>{messages[currentIndex].header}</strong>
        {/if}
        <div class="flex-grow-1"></div>
        <button on:click={() => ($TUTORIAL_VISIBLE = false)} type="button" class="button-icon primary">✕</button>
    </div>
    <div class="tutorial-body">
        <p>{messages[currentIndex].message}</p>
    </div>
    <div class="tutorial-controls">
        <ButtonIcon
            icon="vscode:arrow-left"
            class="primary"
            on:click={() => setMessage(currentIndex - 1)}
            disabled={currentIndex == 0}
        />
        {#if currentIndex < messages.length - 1}
            <ButtonIcon icon="vscode:arrow-right" on:click={() => setMessage(currentIndex + 1)} class="primary" />
        {:else}
            <ButtonIcon icon="vscode:check" on:click={() => setMessage(currentIndex + 1)} class="success" />
        {/if}
    </div>
</div>
