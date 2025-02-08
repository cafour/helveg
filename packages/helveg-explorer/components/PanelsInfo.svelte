<script lang="ts">
    import { AppIcons } from "../const.ts";
    import Icon from "./Icon.svelte";
</script>

<h2 class="extrabold mb-8">Panels</h2>
<dl class="theme-helveg">
    <dt>
        <Icon name={AppIcons.SearchPanel} class="w-16 inline-block" /> Search
    </dt>
    <dd>
        <p>Searches though the nodes and shows the results.</p>
    </dd>

    <dt>
        <Icon name={AppIcons.LayoutPanel} class="w-16 inline-block" /> Layout
    </dt>
    <dd>
        <p>Contains settings affecting what nodes are shown and how they are laid out.</p>
        <p>Allows to pause, restart, or run the layout in the background.</p>
    </dd>

    <dt>
        <Icon name={AppIcons.AppearancePanel} class="w-16 inline-block" /> Appearance
    </dt>
    <dd>
        <p>Contains settings affecting the appearance of the diagram.</p>
    </dd>

    <dt>
        <Icon name={AppIcons.ToolsPanel} class="w-16 inline-block" /> Tools
    </dt>
    <dd>
        <p>Allows you to configure the behaviour of some of the tools.</p>
    </dd>

    <dt>
        <Icon name={AppIcons.PropertiesPanel} class="w-16 inline-block" /> Properties
    </dt>
    <dd>
        <p>Shows metadata of the currently selected node.</p>
    </dd>

    <dt>
        <Icon name={AppIcons.DocumentPanel} class="w-16 inline-block" /> Document
    </dt>
    <dd>
        <p>Contains document metadata and allows exporting the diagram into an image.</p>
    </dd>
</dl>
