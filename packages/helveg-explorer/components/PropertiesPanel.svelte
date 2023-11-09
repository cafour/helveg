<script lang="ts">
    import {
    MultigraphDiagnosticSeverity,
        type MultigraphDiagnostic,
        type MultigraphNode,
    } from "../deps/helveg-diagram.ts";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "../const.ts";
    import Icon from "./Icon.svelte";

    export let node: MultigraphNode | null = null;
    $: nodeItems =
        [
            ...Object.entries(node?.properties ?? {}).filter(
                ([k, v]) => k !== "Diagnostics"
            ),
        ].map((p) => ({
            key: p[0]!,
            value: p[1],
        })) ?? [];
    $: diagnostics = node?.properties?.Diagnostics ?? [];

    function getDiagnosticIcon(diagnostic: MultigraphDiagnostic) {
        switch (diagnostic.severity) {
            case MultigraphDiagnosticSeverity.Hidden:
                return AppIcons.HiddenDiagnostic;
            case MultigraphDiagnosticSeverity.Info:
                return AppIcons.InfoDiagnostic;
            case MultigraphDiagnosticSeverity.Warning:
                return AppIcons.WarningDiagnostic;
            case MultigraphDiagnosticSeverity.Error:
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
