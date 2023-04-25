<script lang="ts">
    import { onMount } from "svelte";

    export let value: string;
    let additionalClass: string = "";
    export { additionalClass as class };

    let initialHeight: number | null;

    function resize(event: KeyboardEvent) {
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
</script>

<textarea bind:value class={additionalClass} on:keyup|preventDefault={resize} />

<style>
    textarea {
        width: 100%;
        height: 100%;
        resize: none;
        overflow: hidden;
    }
</style>
