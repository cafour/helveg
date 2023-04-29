<script lang="ts">
    import { AppIcons, AppTools } from "model/const";
    import RadioGroup, { type RadioItem } from "./RadioGroup.svelte";

    let tools = <RadioItem[]>[
        {
            value: AppTools.ShowProperties,
            label: "ShowProperties",
            icon: AppIcons.ShowPropertiesTool,
            hideLabel: true,
        },
        {
            value: AppTools.Move,
            label: "Move",
            icon: AppIcons.MoveTool,
            hideLabel: true,
        },
        {
            value: AppTools.Toggle,
            label: "Toggle",
            icon: AppIcons.ToggleTool,
            hideLabel: true,
        },
        {
            value: AppTools.Cut,
            label: "Cut",
            icon: AppIcons.CutTool,
            hideLabel: true,
        },
    ];

    export let selectedTool = tools[0].value;
</script>

<div class="toolbox">
    <RadioGroup
        groupName="tools"
        items={tools}
        bind:selected={selectedTool}
        on:change
    />
</div>
