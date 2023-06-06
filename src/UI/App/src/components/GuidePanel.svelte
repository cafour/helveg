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
    
    <h3>About</h3>

    <p>
        <em>Helveg</em> is the product of my studies at the <a href="https://www.fi.muni.cz/" target="_blank">Faculty of Informatics, Masaryk University</a>.
    </p>
    <p>
        The <em>CodePizza</em> feature has been created as a part of the <em>PV097 Visual Creativity Informatics</em> course in spring 2023. <em>CodePizza</em> is dedicated to the now defunct <em>Pizza Nostra</em>, which used to make the weirdest looking pizza I have ever seen.
    </p>
    <p>
        &#169; 2020-2023 Adam Štěpánek and contributors
    </p>
    
    <h3>License</h3>
    <p>
        <em>Helveg</em> is licensed under the <a href="https://opensource.org/license/bsd-3-clause/" target="_blank">3-Clause BSD license</a>.
    </p>
    <p>
        Helveg uses Visual Studio Icons by Microsoft. These are licensed under <a href="https://www.microsoft.com/en-us/download/details.aspx?id=35825" target="_blank">Microsoft Software License Terms</a>.
    </p>
    <p>
        Please note that other restrictions might apply to the visualized data itself.
    </p>
</Panel>
