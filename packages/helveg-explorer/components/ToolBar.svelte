<script lang="ts">
    import { AppIcons, AppTools } from "../const.ts";
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
            value: AppTools.Remove,
            label: "Remove",
            icon: AppIcons.RemoveTool,
            hideLabel: true,
        },
    ];

    export let selectedTool = tools[0].value;
    let className: string | undefined;
    export { className as class };
</script>

<div class="toolbar {className}">
    <RadioGroup
        groupName="tools"
        items={tools}
        bind:selected={selectedTool}
        on:change
    />
</div>
