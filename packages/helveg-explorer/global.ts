import type { Diagram } from "./deps/helveg-diagram.ts";
import Explorer from "./components/Explorer.svelte";

export function createExplorer(diagram: Diagram): Explorer {
    const explorerElement = document.createElement("div");
    explorerElement.classList.add("explorer");
    diagram.element.appendChild(explorerElement);

    const app = new Explorer({
        target: explorerElement,
        props: {
            diagram: diagram
        }
    })

    return app;
}
