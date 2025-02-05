<script lang="ts">
    import type {
        MultigraphDiagnostic,
        MultigraphNode,
    } from "../deps/helveg-diagram.ts";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import Icon from "./Icon.svelte";
    import * as marked from "../deps/marked.ts";
    import dompurify from "../deps/dompurify.ts";
    import NodeInspector from "./NodeInspector.svelte";
    import type { IExplorerState } from "../explorer-state.ts";
    import { getContext } from "svelte";

    const state = getContext<IExplorerState>("state");
    
    export let node: string | null = null;
    $: nodeAttr = state.diagram.graph?.getNodeAttributes(node);
    $: nodeModel = nodeAttr?.model;
    $: nodeItems =
        [
            ...Object.entries(nodeModel ?? {}).filter(
                ([k, v]) =>
                    k !== "diagnostics" &&
                    k !== "comments" &&
                    !k.startsWith("$"),
            ),
        ]
            .map((p) => ({
                key: p[0]!,
                value: p[1],
            }))
            .sort((a, b) => a.key.localeCompare(b.key)) ?? [];
    $: diagnostics = nodeModel?.diagnostics ?? [];
    $: comments = nodeModel?.comments ?? [];

    function getDiagnosticIcon(diagnostic: MultigraphDiagnostic) {
        switch (diagnostic.severity) {
            case "hidden":
                return AppIcons.HiddenDiagnostic;
            case "info":
                return AppIcons.InfoDiagnostic;
            case "warning":
                return AppIcons.WarningDiagnostic;
            case "error":
                return AppIcons.ErrorDiagnostic;
            default:
                return AppIcons.UnknownDiagnostic;
        }
    }
    
    function getDiagnosticClass(diagnostic: MultigraphDiagnostic) {
        switch (diagnostic.severity) {
            case "info":
                return "text-success-500";
            case "warning":
                return "text-warning-500";
            case "error":
                return "text-error-500";
        }
        return "";
    }
</script>

<Panel name="Properties" indent={false} id={AppPanels.Properties}>
    <!-- NB: The inspector is not in the `if` below because of performance. There is a `canvas` in the inspector. -->
    <Subpanel class={node == null ? "hidden" : undefined}>
        <NodeInspector node={nodeAttr} />
    </Subpanel>
    {#if node == null}
        <span class="indent"
            >Click on a node with the <Icon
                name={AppIcons.ShowPropertiesTool}
            /> tool or in the Tree View to view its properties.</span
        >
    {:else}
        {#if comments.length > 0}
            <Subpanel name="Comments" indent={false}>
                {#each comments as comment}
                    <div class="comment flex flex-col gap-2 px-16 pb-16">
                        {#if comment.format == "markdown"}
                            {@html dompurify.sanitize(
                                marked.parse(comment.content).toString(),
                            )}
                        {:else}
                            <p>{comment.content}</p>
                        {/if}
                    </div>
                {/each}
            </Subpanel>
        {/if}
        {#if diagnostics.length > 0}
            <Subpanel name="Diagnostics" indent={true} bodyClass="gap-8">
                {#each diagnostics as diagnostic}
                    <div class="diagnostic flex flex-col gap-4 pb-8">
                        <div class="flex flex-row gap-4 align-items-center">
                            <Icon
                                name={getDiagnosticIcon(diagnostic)}
                                title={diagnostic.severity}
                                additionalClasses={getDiagnosticClass(diagnostic)}
                            />
                            <strong>{diagnostic.id}</strong>
                        </div>
                        <span>{diagnostic.message}</span>
                    </div>
                {/each}
            </Subpanel>
        {/if}
        <Subpanel name="Details">
            <KeyValueList bind:items={nodeItems} />
        </Subpanel>
    {/if}
</Panel>
