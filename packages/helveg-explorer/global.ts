import type { Diagram } from "./deps/helveg-diagram.ts";
import App from "./components/App.svelte";

export function createExplorer(diagram: Diagram): App {
    const explorerElement = document.createElement("div");
    explorerElement.classList.add("explorer");
    diagram.element.appendChild(explorerElement);

    const app = new App({
        target: explorerElement,
        props: {
            diagram: diagram
        }
    })

    return app;
}
