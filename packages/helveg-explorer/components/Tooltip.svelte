<script lang="ts">
    export let text: string;
    export let target: HTMLElement
    
    let tooltip: HTMLElement;
    
    target.addEventListener("mouseover", onMouseover);
    target.addEventListener("mouseleave", onMouseleave);
    
    let timeout = 0;
    
    function onMouseover() {
        timeout = window.setTimeout(() => tooltip.classList.add("visible"), 500);
    }

    function onMouseleave() {
        clearTimeout(timeout);
        tooltip.classList.remove("visible");
    }
</script>

<div class="tooltip" bind:this={tooltip}>{text}</div>
