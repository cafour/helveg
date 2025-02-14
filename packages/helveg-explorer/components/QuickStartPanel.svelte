<script lang="ts">
    import { getContext } from "svelte";
    import { get } from "svelte/store";
    import { AppPanels } from "../const.ts";
    import ButtonStretch from "./ButtonStretch.svelte";
    import Panel from "./Panel.svelte";
    import type { IExplorerState } from "../explorer-state.ts";
    import { NodeColorSchemaPreset } from "../options.ts";
    import { OP_REFRESH } from "../operations/op-layout.ts";

    const state = getContext<IExplorerState>("state");

    async function projectExploration() {
        state.dataOptions.update((dataOptions) => {
            dataOptions.expandedDepth = 1;
            dataOptions.selectedKinds = [
                "Solution",
                "Project",
                "Namespace",
                "Type",
                "Method",
                "Property",
                "Event",
                "Field",
                "TypeParameter",
                "Parameter",
            ];
            dataOptions.selectedRelations = ["declares", "typeOf", "returns", "dependsOn", "inheritsFrom"];
            dataOptions.shouldKeepVisible = false;
            return dataOptions;
        });
        state.appearanceOptions.update((appearanceOptions) => {
            appearanceOptions.nodeColorPreset = NodeColorSchemaPreset.Universal;
            return appearanceOptions;
        });
        (document.activeElement as HTMLElement)?.blur();
        await state.operationExecutor.triggerManually(OP_REFRESH, undefined);
        state.dataOptions.update((dataOptions) => {
            dataOptions.shouldKeepVisible = true;
            return dataOptions;
        });
    }

    async function dependencies() {
        state.dataOptions.update((dataOptions) => {
            dataOptions.expandedDepth = -1;
            dataOptions.selectedKinds = [
                "Project",
                "Package"
            ];
            dataOptions.selectedRelations = ["dependsOn"];
            dataOptions.shouldKeepVisible = false;
            return dataOptions;
        });
        state.appearanceOptions.update((appearanceOptions) => {
            appearanceOptions.nodeColorPreset = NodeColorSchemaPreset.Universal;
            return appearanceOptions;
        });
        (document.activeElement as HTMLElement)?.blur();
        await state.operationExecutor.triggerManually(OP_REFRESH, undefined);
        state.dataOptions.update((dataOptions) => {
            dataOptions.shouldKeepVisible = true;
            return dataOptions;
        });
    }

    async function allTypes() {
        state.dataOptions.update((dataOptions) => {
            dataOptions.expandedDepth = -1;
            dataOptions.selectedKinds = [
                "Solution",
                "Project",
                "Namespace",
                "Type",
                "TypeParameter"
            ];
            dataOptions.selectedRelations = ["declares", "inheritsFrom"];
            dataOptions.shouldKeepVisible = false;
            return dataOptions;
        });
        state.appearanceOptions.update((appearanceOptions) => {
            appearanceOptions.nodeColorPreset = NodeColorSchemaPreset.TypeFocus;
            return appearanceOptions;
        });
        (document.activeElement as HTMLElement)?.blur();
        await state.operationExecutor.triggerManually(OP_REFRESH, undefined);
        state.dataOptions.update((dataOptions) => {
            dataOptions.shouldKeepVisible = true;
            return dataOptions;
        });
    }

    async function birdseyeView() {
        state.dataOptions.update((dataOptions) => {
            dataOptions.expandedDepth = -1;
            dataOptions.selectedKinds = [
                "Solution",
                "Project",
                "Namespace",
                "Type",
                "Method",
                "Property",
                "Event",
                "Field",
                "TypeParameter",
                "Parameter",
            ];
            dataOptions.selectedRelations = ["declares", "typeOf", "returns", "dependsOn", "inheritsFrom"];
            dataOptions.shouldKeepVisible = false;
            return dataOptions;
        });
        state.appearanceOptions.update((appearanceOptions) => {
            appearanceOptions.nodeColorPreset = NodeColorSchemaPreset.Universal;
            return appearanceOptions;
        });
        (document.activeElement as HTMLElement)?.blur();
        await state.operationExecutor.triggerManually(OP_REFRESH, undefined);
        state.dataOptions.update((dataOptions) => {
            dataOptions.shouldKeepVisible = true;
            return dataOptions;
        });
    }
</script>

<Panel name="Quick Start" indent={true} id={AppPanels.QuickStart}>
    <div class="flex flex-col gap-16">
        <ButtonStretch
            class="primary"
            on:click={projectExploration}
            hint="Diagram with the Solution and its Projects. Can be expanded all the way to method parameters. Doesn't include dependencies."
        >
            Project exploration
        </ButtonStretch>
        <ButtonStretch
            class="primary"
            on:click={dependencies}
            hint="Diagram with only Projects and Packages and their dependencies. Doesn't include anything else."
        >
            Project dependencies
        </ButtonStretch>
        <ButtonStretch
            class="primary"
            on:click={allTypes}
            hint="Completely expanded graph of all Projects, Namespaces and Types. Doesn't include dependencies."
        >
            All types
        </ButtonStretch>
        <ButtonStretch
            class="primary"
            on:click={birdseyeView}
            hint="Completely expanded graph of the Solution and its Projects along with their contents all the way to method parameters. Doesn't include dependencies."
        >
            Bird's-eye view
        </ButtonStretch>
    </div>
</Panel>
