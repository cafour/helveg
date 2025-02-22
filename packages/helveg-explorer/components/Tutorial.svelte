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

    function setIndex(value: number): void {
        localStorage.setItem("Tutorial.index", value.toString());
    }

    function getIndex(): number {
        const stored = localStorage.getItem("Tutorial.index");
        return Number.parseInt(stored ?? "0");
    }

    export const TUTORIAL_INDEX = writable(getIndex());
    TUTORIAL_INDEX.subscribe((v) => {
        setIndex(v);
    });
</script>

<script lang="ts">
    import { TUTORIAL_MESSAGES } from "../tutorial.ts";
    import { setPopupPosition } from "../popups.ts";
    import { onMount } from "svelte";
    import ButtonIcon from "./ButtonIcon.svelte";
    import Icon from "./Icon.svelte";
    import Dock from "./Dock.svelte";

    const messages = TUTORIAL_MESSAGES;

    function setMessage(index: number) {
        if (index >= TUTORIAL_MESSAGES.length) {
            $TUTORIAL_VISIBLE = false;
            $TUTORIAL_INDEX = 0;
            return;
        }

        $TUTORIAL_INDEX = index;
        
        if (messages[$TUTORIAL_INDEX].selectedPanel != undefined) {
            dock.setTab(messages[$TUTORIAL_INDEX].selectedPanel ?? null);
        }
        
        requestAnimationFrame(() => {
            const position = messages[$TUTORIAL_INDEX].position;
            if (position.elementQuery) {
                setPopupPosition(element, document.querySelector(position.elementQuery)!);
            } else {
                element.style.top = position.top ?? "";
                element.style.bottom = position.bottom ?? "";
                element.style.left = position.left ?? "";
                element.style.right = position.right ?? "";
                element.classList.remove("arrow-top", "arrow-bottom", "arrow-left", "arrow-right");
            }
        });
    }

    onMount(() => {
        setMessage(getIndex());
    });

    let element: HTMLElement;
    
    export let dock: Dock;
</script>

<div class="tutorial" bind:this={element} class:hidden={!$TUTORIAL_VISIBLE}>
    <div class="tutorial-header">
        {#if messages[$TUTORIAL_INDEX].icon != undefined}
            <Icon name={messages[$TUTORIAL_INDEX].icon} />
        {/if}
        {#if messages[$TUTORIAL_INDEX].header != null}
            <strong>{messages[$TUTORIAL_INDEX].header}</strong>
        {/if}
        <div class="flex-grow-1"></div>
        <button on:click={() => ($TUTORIAL_VISIBLE = false)} type="button" class="button-icon primary">✕</button>
    </div>
    <div class="tutorial-body">
        <p>{messages[$TUTORIAL_INDEX].message}</p>
    </div>
    <div class="tutorial-controls">
        <ButtonIcon
            icon="vscode:arrow-left"
            class="primary"
            on:click={() => setMessage($TUTORIAL_INDEX - 1)}
            disabled={$TUTORIAL_INDEX == 0}
        />
        {#if $TUTORIAL_INDEX < messages.length - 1}
            <ButtonIcon icon="vscode:arrow-right" on:click={() => setMessage($TUTORIAL_INDEX + 1)} class="primary" />
        {:else}
            <ButtonIcon icon="vscode:check" on:click={() => setMessage($TUTORIAL_INDEX + 1)} class="success" />
        {/if}
    </div>
</div>
