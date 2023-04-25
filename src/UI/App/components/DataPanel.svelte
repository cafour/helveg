<script lang="ts">
    import { select_multiple_value } from "svelte/internal";
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
    import RadioGroup from "./RadioGroup.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { SearchMode, type DataOptions } from "model/options";
    import { AppIcons } from "model/const";

    export let dataOptions: DataOptions;

    let searchModes = [
        {
            value: SearchMode.Contains,
            icon: AppIcons.ContainsMode,
        },
        {
            value: SearchMode.Regex,
            icon: AppIcons.RegexMode,
        },
        {
            value: SearchMode.JavaScript,
            icon: AppIcons.JavaScriptMode,
        },
    ];
    let selectedSearchMode: SearchMode = SearchMode.Contains;
    let searchText: string = "";
</script>

<Panel name="Data" indent={false}>
    <div class="indent">
        <em class="mb-16 block">This panel is currently non-functional.</em>
        <button on:click={() => {}} class="button-stretch"
            >Refresh Diagram</button
        >
    </div>
    <Subpanel name="Search">
        <div class="flex flex-row gap-4">
            <input type="text" bind:value={searchText} />
            <RadioGroup
                groupName="searchMode"
                items={searchModes}
                bind:selected={selectedSearchMode}
                class=light
            />
        </div>
    </Subpanel>
    <!-- <Subpanel name="IncludedKinds">
        {#each dataOptions.kinds as kind}
            <label>
                <input
                    type="checkbox"
                    bind:group={dataOptions.selectedKinds}
                    value={kind}
                />
                <Icon name={dataOptions.defaultIcons[kind] ?? dataOptions.fallbackIcon} />
                {kind}
            </label>
        {/each}
    </Subpanel> -->
</Panel>
