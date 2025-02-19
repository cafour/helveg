<script lang="ts">
    import {
        sortProps,
        type HelvegNodeAttributes,
        type MultigraphDiagnostic,
        type MultigraphNode,
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

    interface PropGroup {
        name: string;
        props: { name: string; value: any }[];
    }

    export let node: string | null = null;
    $: nodeAttr = node != null ? state.diagram.graph?.getNodeAttributes(node) : undefined;
    $: propGroups = getPropGroups(nodeAttr);

    function getPropGroups(node: HelvegNodeAttributes | undefined): PropGroup[] {
        if (node == null) {
            return [];
        }

        const propNames = Object.keys(node.model).filter(
            (k) => k !== "diagnostics" && k !== "comments" && !k.startsWith("$"),
        );
        const categories = sortProps(propNames);
        return categories.map<PropGroup>((c) => {
            return {
                name: c[0],
                props: c[1].map((p) => {
                    return {
                        name: p,
                        value: node.model[p],
                    };
                }),
            };
        });
    }
    // $: nodeAttr = node ? state.diagram.graph?.getNodeAttributes(node) : undefined;
    // $: nodeModel = nodeAttr?.model;
    // $: nodeItems =

    //         .reduce()
    //         .map((p) => ({
    //             key: p[0]!,
    //             value: p[1],
    //         }))
    //         .sort((a, b) => a.key.localeCompare(b.key)) ?? [];
    $: diagnostics = nodeAttr?.model.diagnostics ?? [];
    $: comments = nodeAttr?.model.comments ?? [];

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
            >Click on a node with the <Icon name={AppIcons.ShowPropertiesTool} /> tool or in the Tree View to view its properties.</span
        >
    {:else}
        {#if comments.length > 0}
            <Subpanel name="Comments" indent={false}>
                {#each comments as comment}
                    <div class="comment flex flex-col gap-2 px-16 pb-16">
                        {#if comment.format == "markdown"}
                            {@html dompurify.sanitize(marked.parse(comment.content).toString())}
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
                                color="currentColor"
                                class={getDiagnosticClass(diagnostic)}
                            />
                            <strong>{diagnostic.id}</strong>
                        </div>
                        <span>{diagnostic.message}</span>
                    </div>
                {/each}
            </Subpanel>
        {/if}
        <Subpanel name="Details" indent={false}>
            {#each propGroups as group (group.name)}
                <div class="flex flex-col border-b-1" style="border-color: var(--color-surface-200);">
                    <strong
                        class="border-b-1 px-8 pt-8 pb-2"
                        style="border-color: var(--color-surface-200);"
                        >{group.name}</strong
                    >
                    {#each group.props as prop, index (prop.name)}
                        <div
                            class="flex flex-row flex-wrap justify-content-between px-8 py-2"
                            style="background-color: {index % 2 == 0
                                ? 'var(--color-surface-100)'
                                : 'var(--color-surface-75)'};"
                        >
                            <span class="pr-32">{prop.name}</span>
                            <span class="overflow-hidden ellipsis space-nowrap monospace" title={prop.value}>
                                {prop.value}
                            </span>
                        </div>
                    {/each}
                </div>
            {/each}
        </Subpanel>
    {/if}
</Panel>
