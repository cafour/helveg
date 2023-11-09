import type { Diagram } from "./deps/helveg-diagram.ts";
import Explorer from "./components/Explorer.svelte";
import "./styles/explorer.scss";

export function createExplorer(diagram: Diagram): Explorer {
    const explorerElement = document.createElement("div");
    explorerElement.classList.add("explorer");
    explorerElement.style.width = "100%";
    explorerElement.style.height = "100%";
    explorerElement.style.position = "absolute";
    explorerElement.style.top = "0";
    explorerElement.style.left = "0";
    explorerElement.style.margin = "0";
    explorerElement.style.padding = "0";

    diagram.element.appendChild(explorerElement);

    const app = new Explorer({
        target: explorerElement,
        props: {
            diagram: diagram
        }
    })

    return app;
}
