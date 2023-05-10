<script lang="ts">
    import { DiagnosticSeverity, type Diagnostic, type Node } from "model/multigraph";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppIcons, AppPanels } from "model/const";
    import Icon from "./Icon.svelte";

    export let node: Node | null = null;
    $: nodeItems =
        [
            ...Object.entries(node?.properties ?? {}).filter(([k, v]) => k !== "Diagnostics")
        ].map((p) => ({
            key: p[0]!,
            value: p[1],
        })) ?? [];
    $: diagnostics = node?.properties?.Diagnostics ?? [];
    
    function getDiagnosticIcon(diagnostic: Diagnostic) {
        switch (diagnostic.severity) {
            case DiagnosticSeverity.Hidden:
                return AppIcons.HiddenDiagnostic;
            case DiagnosticSeverity.Info:
                return AppIcons.InfoDiagnostic;
            case DiagnosticSeverity.Warning:
                return AppIcons.WarningDiagnostic
            case DiagnosticSeverity.Error:
                return AppIcons.ErrorDiagnostic;
            default:
                return AppIcons.UnknownDiagnostic;
        }
    }
</script>

<Panel name="Properties" indent={false} id={AppPanels.Properties}>
    {#if node == null}
        <span class="indent">Click on a node with the <Icon name={AppIcons.ShowPropertiesTool} /> tool to view its properties.</span>
    {:else}
        <Subpanel>
            <KeyValueList bind:items={nodeItems} />
        </Subpanel>
        {#if diagnostics.length > 0}
            <Subpanel name="Diagnostics" indent={false}>
                {#each diagnostics as diagnostic}
                <div class="diagnostic flex flex-row gap-2 mb-2">
                    <Icon name={getDiagnosticIcon(diagnostic)} title={diagnostic.severity} />
                    <strong>{diagnostic.id}</strong>
                    <span>{diagnostic.message}</span>
                </div>
                {/each}
            </Subpanel>
        {/if}
    {/if}
</Panel>
