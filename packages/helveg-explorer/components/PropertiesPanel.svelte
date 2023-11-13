<script lang="ts">
    import type { DataModel, MultigraphDiagnostic, MultigraphNode } from "../deps/helveg-diagram.ts";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import Icon from "./Icon.svelte";
    import * as marked from "../deps/marked.ts";
    import dompurify from "../deps/dompurify.ts";

    export let node: MultigraphNode | null = null;
    $: nodeItems =
        [
            ...Object.entries(node ?? {}).filter(
                ([k, v]) => k !== "diagnostics" && k !== "comments" && !k.startsWith("$")
            ),
        ].map((p) => ({
            key: p[0]!,
            value: p[1],
        })) ?? [];
    $: diagnostics = node?.diagnostics ?? [];
    $: comments = node?.comments ?? [];

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
</script>

<Panel name="Properties" indent={false} id={AppPanels.Properties}>
    {#if node == null}
        <span class="indent"
            >Click on a node with the <Icon
                name={AppIcons.ShowPropertiesTool}
            /> tool to view its properties.</span
        >
    {:else}
        <Subpanel>
            <KeyValueList bind:items={nodeItems} />
        </Subpanel>
        {#if comments.length > 0}
            <Subpanel name="Comments" indent={false}>
                {#each comments as comment}
                    <div class="comment flex flex-col gap-2 px-16">
                        {#if comment.format == "markdown"}
                            {@html dompurify.sanitize(marked.parse(comment.content))}
                        {:else}
                            <p>{comment.content}</p>
                        {/if}
                    </div>
                {/each}
            </Subpanel>
        {/if}
        {#if diagnostics.length > 0}
            <Subpanel name="Diagnostics" indent={false}>
                {#each diagnostics as diagnostic}
                    <div class="diagnostic flex flex-row gap-2 mb-2">
                        <Icon
                            name={getDiagnosticIcon(diagnostic)}
                            title={diagnostic.severity}
                        />
                        <strong>{diagnostic.id}</strong>
                        <span>{diagnostic.message}</span>
                    </div>
                {/each}
            </Subpanel>
        {/if}
    {/if}
</Panel>
