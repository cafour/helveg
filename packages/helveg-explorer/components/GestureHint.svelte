<script lang="ts">
    import { getModifiersHint, getMouseButtonHint, type MouseGesture } from "../operations";

    export let gesture: MouseGesture;
</script>

<div>
    {#if gesture.modifiers != null}
        <span class="keycap">{getModifiersHint(gesture.modifiers)}</span>
        <span> + </span>
    {/if}
    <span>{getMouseButtonHint(gesture.button, gesture.isDouble ?? false)}</span>
</div>
