<script lang="ts">
    import { getContext } from "svelte";
    import { setPopupPosition } from "../popups";

    export let text: string;
    export let header: string | undefined = undefined;
    export let shortcut: string | undefined = undefined;
    export let target: HTMLElement | null;
    export let delay: number = 0;

    let lastTarget: HTMLElement | null = null;
    let originalTitle: string | null = null;

    const tooltip = document.createElement("div");
    tooltip.classList.add("tooltip", "hidden");

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
        setPopupPosition(tooltip, target!);
    }

    function hide() {
        tooltip.classList.add("hidden");
    }
</script>
