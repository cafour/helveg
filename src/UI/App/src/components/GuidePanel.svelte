<script lang="ts">
    import { AppIcons, AppPanels } from "model/const";
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
</script>

<Panel name="Guide" id={AppPanels.Guide}>
    <a href="https://helveg.net/docs/" class="button-stretch" target="_blank"
        >Documentation</a
    >

    <h3>Panels</h3>
    <dl class="theme-helveg">
        <dt>
            <Icon name={AppIcons.DataPanel} /> Data
        </dt>
        <dd>
            <p>Filters the nodes and relations shown.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.LayoutPanel} /> Layout
        </dt>
        <dd>
            <p>Contains settings affecting how the nodes are laid out.</p>
            <p>
                Allows to pause, restart, or run the layout in the background.
            </p>
        </dd>

        <dt>
            <Icon name={AppIcons.AppearancePanel} /> Appearance
        </dt>
        <dd>
            <p>Contains settings affecting the appearance of the diagram.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.ToolsPanel} /> Tools
        </dt>
        <dd>
            <p>Allows you to configure the behaviour of some of the tools.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.PropertiesPanel} /> Properties
        </dt>
        <dd>
            <p>Shows metadata of the currently selected node.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.DocumentPanel} /> Document
        </dt>
        <dd>
            <p>
                Contains document metadata and allows exporting the diagram into
                an image.
            </p>
        </dd>
    </dl>

    <h3>Tools</h3>
    <dl class="theme-helveg">
        <dt>
            <Icon name="base:ShowAllConfigurations" /> ShowProperties
        </dt>
        <dd>
            <p>
                If a node is clicked, while this tool is active, the properties
                of the node show up in the Properties panel.
            </p>
        </dd>

        <dt>
            <Icon name="base:MoveGlyph" /> Move
        </dt>
        <dd>
            <p>Moves the node under the cursor.</p>
        </dd>

        <dt>
            <Icon name="base:Branch" /> Toggle
        </dt>
        <dd>
            <p>
                Appends child nodes to the node under the cursor with respect to
                the <code>CollapsingRelation</code>
                in the Tools panel.
            </p>
        </dd>

        <dt>
            <Icon name="base:Cut" /> Cut
        </dt>
        <dd>
            <p>
                Cuts the selected node and its descendants from the diagram. The
                child relation is the <code>CuttingRelation</code> in the Tools
                panel. If <code>IsCuttingTransitive</code> is false, only the node
                under the cursor is removed.
            </p>
        </dd>
    </dl>

    <h3>Glyphs</h3>
    <p>
        Glyphs are nodes of the diagram. The icon in the center represents the
        kind of the visualized entity. The outlines symbolize, in order from
        innermost to outermost:
    </p>
    <ul>
        <li>
            <strong>"Staticness"</strong>: If the line is dashed, the entity is
            static; if it is solid, it is tied to an instance.
        </li>
        <li>
            <strong>Instance member count</strong>: The number of instance
            members inside a type entity. Always solid.
        </li>
        <li>
            <strong>Static member count</strong>: The number of static members
            inside a type entity. Always dashed.
        </li>
    </ul>

    <p>
        If a node is on fire, there's an error in the entity it represents. The
        error is shown in the Properties panel. Similarly, if a node is
        smoldering, there's a warning.
    </p>
</Panel>
