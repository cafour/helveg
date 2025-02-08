<script lang="ts">
    import { AppIcons, AppPanels } from "../const.ts";
    import Icon from "./Icon.svelte";
    import Panel from "./Panel.svelte";
</script>

<Panel name="Guide" id={AppPanels.Guide}>
    <a
        href="https://helveg.net/user-guide/"
        class="button-stretch primary"
        target="_blank">Documentation</a
    >

    <h3>Panels</h3>
    <dl class="theme-helveg">
        <dt>
            <Icon name={AppIcons.SearchPanel} /> Search
        </dt>
        <dd>
            <p>Searches though the nodes and shows the results.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.LayoutPanel} /> Layout
        </dt>
        <dd>
            <p>
                Contains settings affecting what nodes are shown and how they
                are laid out.
            </p>
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
            <Icon name={AppIcons.ShowPropertiesTool} /> ShowProperties
        </dt>
        <dd>
            <p>
                If a node is clicked, while this tool is active, the properties
                of the node show up in the Properties panel.
            </p>
        </dd>

        <dt>
            <Icon name={AppIcons.MoveTool} /> Move
        </dt>
        <dd>
            <p>Moves the node under the cursor.</p>
        </dd>

        <dt>
            <Icon name={AppIcons.ToggleTool} /> Toggle
        </dt>
        <dd>
            <p>
                Appends child nodes to the node under the cursor with respect to
                the <code>Relation</code> setting in the Tools panel.
            </p>
        </dd>

        <dt>
            <Icon name={AppIcons.RemoveTool} /> Remove
        </dt>
        <dd>
            <p>
                Removes the selected node and its descendants from the diagram.
                The child relation is the <code>Relation</code> setting in the
                Tools panel. If <code>IsTransitive</code> is false, only the node
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

    <h3>About</h3>

    <p>
        <em>Helveg</em> is the product of my studies at the
        <a href="https://www.fi.muni.cz/" target="_blank"
            >Faculty of Informatics, Masaryk University</a
        >.
    </p>
    <p>
        The <em>CodePizza</em> feature has been created as a part of the
        <em>PV097 Visual Creativity Informatics</em>
        course in spring 2023. <em>CodePizza</em> is dedicated to the now
        defunct <em>Pizza Nostra</em>, which used to make the weirdest looking
        pizza I have ever seen.
    </p>
    <p>&#169; 2020-2025 Adam Štěpánek</p>

    <h3>License</h3>
    <p>
        <em>Helveg</em> is licensed under the
        <a href="https://opensource.org/license/bsd-3-clause/" target="_blank"
            >3-Clause BSD license</a
        >.
    </p>
    <p>
        Helveg uses <code
            ><a
                href="https://github.com/microsoft/vscode-codicons"
                target="_blank">vscode-codicons</a
            ></code
        >
        by Microsoft, which are licensed under the
        <a
            href="https://creativecommons.org/licenses/by/4.0/legalcode"
            target="_blank"
            >Creative Commons Attribution 4.0 International Public License</a
        >.
    </p>
    <p>
        Please note that other restrictions might apply to the visualized data
        itself.
    </p>
</Panel>
