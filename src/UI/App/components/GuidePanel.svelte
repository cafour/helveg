<script lang="ts">
    import { AppIcons } from "model/const";
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
    import GlyphsPanel from "./AppearancePanel.svelte";
    import PropertiesPanel from "./PropertiesPanel.svelte";
    import DocumentPanel from "./DocumentPanel.svelte";
</script>

<Panel name="Guide">
    <a href="https://helveg.net/docs/" class="button-stretch" target="_blank"
        >Documentation</a
    >

    <h3>Panels</h3>

    <dl>
        <dt>
            <Icon name={AppIcons.DataPanel} /> Data
        </dt>
        <dd>Filters the nodes and relations shown.</dd>

        <dt>
            <Icon name={AppIcons.LayoutPanel} /> Layout
        </dt>
        <dd>
            Contains settings affecting how the nodes are laid out. <br />
            Allows to pause, restart, or run the layout in the background.
        </dd>

        <dt>
            <Icon name={AppIcons.AppearancePanel} /> Appearance
        </dt>
        <dd>Contains settings affecting the appearance of the diagram.</dd>

        <dt>
            <Icon name={AppIcons.PropertiesPanel} /> Properties
        </dt>
        <dd>Shows metadata of the currently selected node.</dd>

        <dt>
            <Icon name={AppIcons.DocumentPanel} /> Document
        </dt>
        <dd>
            Contains document metadata. <br />
            Allows the export into an image.
        </dd>
    </dl>

    <!-- <h3>Tools</h3>
    <dl>
        <dt>
            <Icon name="base:ShowAllConfigurations" /> ShowProperties
        </dt>
        <dd>
            Selects the node under the cursor and shows its properties in the
            Properties panel.
        </dd>

        <dt>
            <Icon name="base:MoveGlyph" /> Move
        </dt>
        <dd>Moves the node under the cursor.</dd>

        <dt>
            <Icon name="base:MoveGlyph" /> Expand
        </dt>
        <dd>
            Appends child nodes to the node under the cursor with respect to a
            relation selected in the Data panel.
        </dd>

        <dt>
            <Icon name="base:Cut" /> Saw
        </dt>
        <dd>
            Cuts the selected node, its relations, and any would-be orphan nodes
            from the diagram.
        </dd>
    </dl> -->
</Panel>
