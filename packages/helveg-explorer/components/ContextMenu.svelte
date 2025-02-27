<script lang="ts">
    import { getContext } from "svelte";
    import type { IExplorerState } from "../explorer-state";
    import { OperationScope, type NodeOperation } from "../operations/index.ts";
    import Icon from "./Icon.svelte";

    const state = getContext<IExplorerState>("state");
    const nodeOperations = state.operationExecutor.getOperations(OperationScope.NODE)
        .filter((op) => !op.hidden)
        .sort((a, b) => a.name.localeCompare(b.name));

    let isVisible = false;
    let posX = -1;
    let posY = -1;
    let sizeX = -1;
    let sizeY = -1;
    let contextNode: string | null = null;

    state.diagram.element.addEventListener("contextmenu", (e) => {
        if ((e.target as HTMLElement | undefined)?.tagName !== "CANVAS") {
            isVisible = false;
            return;
        }

        isVisible = true;
        posX = e.clientX;
        posY = e.clientY;
        contextNode = state.diagram.hoveredNode;

        // display above the cursor if there's not enough space below it
        if (window.innerHeight - posY < sizeY) {
            posY = posY - sizeY;
        }

        e.preventDefault();
    });

    state.diagram.element.addEventListener("click", (e) => {
        isVisible = false;
    });

    function onCreated(element: HTMLElement) {
        sizeX = element.offsetWidth;
        sizeY = element.offsetHeight;
    }

    async function onClicked(op: NodeOperation, e: MouseEvent) {
        if (contextNode && op.beginExecute) {
            await op.beginExecute(state, contextNode, {
                modifiers: state.operationExecutor.pressedModifiers,
                type: "mouseDown",
            });
        }
    }
</script>

{#if isVisible}
    <div use:onCreated class="context-menu" style="position: absolute; top: {posY}px; left: {posX}px;">
        <ul>
            {#each nodeOperations as operation}
                <li>
                    <button on:click={(e) => onClicked(operation, e)}>
                        {#if operation.icon}
                            <Icon name={operation.icon} />
                        {/if}
                        <span>{operation.name}</span>
                    </button>
                </li>
            {/each}
        </ul>
    </div>
{/if}
