<script lang="ts">
    import { getContext } from "svelte";

    export let text: string;
    export let header: string | undefined = undefined;
    export let target: HTMLElement | null;
    export let delay: number = 0;

    let lastTarget: HTMLElement | null = null;
    let originalTitle: string | null = null;

    const tooltip = document.createElement("div");
    tooltip.classList.add("tooltip");
    const rootElement = getContext<HTMLElement>("rootElement");
    rootElement.appendChild(tooltip);
    const headerElement = tooltip.appendChild(document.createElement("strong"));
    headerElement.style.display = "none";
    const paragraphElement = tooltip.appendChild(document.createElement("p"));
    $: paragraphElement.innerText = text;
    
    $: if (header != null) {
        headerElement.innerText = header;
        headerElement.style.display = "inline";
    }

    $: {
        if (target) {
            if (lastTarget) {
                lastTarget.removeEventListener("mouseenter", onMouseenter);
                lastTarget.removeEventListener("mouseleave", onMouseleave);

                if (originalTitle) {
                    lastTarget.title = originalTitle;
                }
            }

            lastTarget = target;
            originalTitle = lastTarget.title;

            target.addEventListener("mouseenter", onMouseenter);
            target.addEventListener("mouseleave", onMouseleave);
            target.title = "";
        }
    }

    let timeout = 0;

    function onMouseenter() {
        if (delay > 0) {
            timeout = window.setTimeout(() => show(), delay);
        } else {
            show();
        }
    }

    function onMouseleave() {
        clearTimeout(timeout);
        hide();
    }

    function show() {
        const rect = target!.getBoundingClientRect();

        tooltip.style.left = rect.x + rect.width / 2 - tooltip.clientWidth / 2 + "px";

        if (tooltip.clientHeight > rect.y) {
            tooltip.classList.add("arrow-top");
            tooltip.classList.remove("arrow-bottom");
            tooltip.style.top = rect.y + rect.height + "px";
        } else {
            tooltip.classList.add("arrow-bottom");
            tooltip.classList.remove("arrow-top");
            tooltip.style.top =
                rect.y - rect.height - tooltip.offsetHeight + "px";
        }

        tooltip.classList.add("visible");
    }

    function hide() {
        tooltip.classList.remove("visible");
    }
</script>
