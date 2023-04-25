<script lang="ts">
    import { onMount } from "svelte";

    export let value: string;
    let additionalClass: string = "";
    export { additionalClass as class };

    let initialHeight: number | null;

    function onKeyup(event: KeyboardEvent) {
        if (!event.target) {
            return;
        }

        let textarea = event.target as HTMLTextAreaElement;

        if (!initialHeight) {
            initialHeight = parseInt(
                window.getComputedStyle(textarea).height,
                10
            );
        }

        let lineHeight = parseInt(
            window.getComputedStyle(textarea).lineHeight,
            10
        );
        textarea.style.height = `${Math.max(
            initialHeight,
            textarea.scrollHeight
        )}px`;
    }

    function onKeydown(event: KeyboardEvent) {
        let textarea = event.target as HTMLTextAreaElement;
        if (textarea && event.key === "Enter") {
            event.preventDefault();
            textarea.form?.dispatchEvent(new Event("submit", { bubbles: false, cancelable: true }));
        }
    }
</script>

<textarea
    bind:value
    class={additionalClass}
    on:keyup|preventDefault={onKeyup}
    on:keydown={onKeydown}
/>

<style>
    textarea {
        width: 100%;
        height: 100%;
        resize: none;
        overflow: hidden;
    }
</style>
