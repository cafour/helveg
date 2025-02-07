<script lang="ts">
    import { getContext } from "svelte";

    export let text: string;
    export let header: string | undefined = undefined;
    export let shortcut: string | undefined = undefined;
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
    headerElement.classList.add("tooltip-header");

    const strongElement = headerElement.appendChild(document.createElement("strong"));

    const shortcutElement = headerElement.appendChild(document.createElement("span"));
    shortcutElement.style.display = "none";
    shortcutElement.classList.add("keycap");

    const paragraphElement = tooltip.appendChild(document.createElement("p"));
    $: paragraphElement.innerText = text;

    $: if (header != null) {
        strongElement.innerText = header;
        headerElement.style.display = "";
    }
    
    $: if (shortcut != null) {
        shortcutElement.innerText = shortcut;
        shortcutElement.style.display = "";
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

        if (tooltip.clientHeight > rect.y) {
            tooltip.classList.add("arrow-top");
            tooltip.classList.remove("arrow-bottom");
            tooltip.style.top = rect.y + rect.height + "px";
        } else {
            tooltip.classList.add("arrow-bottom");
            tooltip.classList.remove("arrow-top");
            tooltip.style.top = rect.y - rect.height - tooltip.offsetHeight + "px";
        }

        const xMid = rect.x + rect.width / 2;
        if (tooltip.clientWidth / 2 > xMid) {
            tooltip.style.left = `calc(${xMid}px - 0.5 * var(--arrow-size))`;
            tooltip.classList.add("point-left");
        } else if (tooltip.clientWidth / 2 > document.documentElement.clientWidth - xMid) {
            tooltip.style.left = `calc(${xMid - tooltip.clientWidth}px + 0.5 * var(--arrow-size))`;
            tooltip.classList.add("point-right");
        } else {
            tooltip.style.left = xMid - tooltip.clientWidth / 2 + "px";
            tooltip.classList.remove("point-left");
            tooltip.classList.remove("point-right");
        }

        tooltip.classList.add("visible");
    }

    function hide() {
        tooltip.classList.remove("visible");
    }
</script>
