<script lang="ts">
    import { AppIcons } from "../const";
    import Icon from "./Icon.svelte";
    import Tooltip from "./Tooltip.svelte";

    export let text: string;

    let icon: HTMLElement;
</script>

<div class="hint">
    <Tooltip bind:target={icon} {text} />
    <Icon name={AppIcons.Hint} bind:element={icon} title="" />
</div>
