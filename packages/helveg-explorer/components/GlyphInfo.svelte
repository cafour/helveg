<script lang="ts">
    import type { Readable } from "svelte/store";
    import {
        EntityKind,
        LimitedTypeKind,
        MemberAccessibility,
        MethodKind,
        TypeKind,
        type DataModel,
        type Diagram,
    } from "../deps/helveg-diagram.ts";
    import { getContext } from "svelte";
    import NodeKindIcon from "./NodeKindIcon.svelte";
    import CHEATSHEET_DONUT from "../img/cheatsheet_donut.svg";
    import CHEATSHEET_CONTOURS from "../img/cheatsheet_contours.svg";
    import CHEATSHEET_EFFECTS from "../img/cheatsheet_effects.svg";

    let diagram = getContext<Diagram>("diagram");
    let model = getContext<Readable<DataModel>>("model");

    const nodeKinds = diagram.options.nodeKindOrder;

    const staticKinds = [
        { nodeKind: EntityKind.Type, typeKind: TypeKind.Class },
        { nodeKind: EntityKind.Method },
        { nodeKind: EntityKind.Property },
        { nodeKind: EntityKind.Field },
        { nodeKind: EntityKind.Event },
    ];

    const specialGlyphs = [
        { nodeKind: EntityKind.Field, isConst: true, name: "Constant field" },
        { nodeKind: EntityKind.Field, isEnumItem: true, name: "Enum member field" },
        { nodeKind: EntityKind.Method, methodKind: MethodKind.Constructor, name: "Constructor" },
        { nodeKind: EntityKind.Method, methodKind: MethodKind.Destructor, name: "Destructor" },
        { nodeKind: EntityKind.Method, methodKind: MethodKind.UserDefinedOperator, name: "Operator" },
        { nodeKind: EntityKind.Type, typeKind: TypeKind.Class, isRecord: true, name: "Record class" },
        { nodeKind: EntityKind.Type, typeKind: TypeKind.Struct, isRecord: true, name: "Record struct" },
        { nodeKind: EntityKind.Property, isIndexer: true, name: "Indexer property" },
    ];
</script>

<div class="grid grid-cols-2">
    <!-- Left column -->
    <div class="flex flex-col gap-16">
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Entities</strong>
            {#each nodeKinds as kind}
                <div class="flex flex-row align-items-center gap-8">
                    <NodeKindIcon nodeKind={kind} class="w-32" />
                    <span>{kind}</span>
                </div>
            {/each}
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Specials</strong>
            <span class="text-xs">Specific common combinations of kinds and properties have specialized icons:</span>
            {#each specialGlyphs as special}
                <div class="flex flex-row align-items-center gap-8">
                    <NodeKindIcon {...special} class="w-32" />
                    <span>{special.name}</span>
                </div>
            {/each}
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Errors and warnings</strong>
            <span class="text-xs">Compiler diagnostics are represented by animated effects:</span>
            {@html CHEATSHEET_EFFECTS}
        </div>
    </div>

    <!-- Right column -->
    <div class="flex flex-col gap-16">
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Types</strong>
            {#each Object.keys(LimitedTypeKind) as typeKind}
                <div class="flex flex-row align-items-center gap-8">
                    <NodeKindIcon nodeKind="Type" {typeKind} class="w-32" />
                    <span>{typeKind}</span>
                </div>
            {/each}
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold mt-8">Accessibility</strong>
            <span class="text-xs">
                C# accessibility modifiers are represented by smaller icons in the lower left corner. For example on
                class:
            </span>
            <div class="flex flex-row align-items-center gap-8">
                <NodeKindIcon nodeKind="Type" typeKind="Class" class="w-48" />
                <code class="text-md">public</code>
                <span>(has no icon)</span>
            </div>
            <div class="flex flex-row align-items-center gap-8">
                <NodeKindIcon
                    nodeKind="Type"
                    typeKind="Class"
                    accessibility={MemberAccessibility.Private}
                    class="w-48"
                />
                <code class="text-md">private</code>
            </div>
            <div class="flex flex-row align-items-center gap-8">
                <NodeKindIcon
                    nodeKind="Type"
                    typeKind="Class"
                    accessibility={MemberAccessibility.Protected}
                    class="w-48"
                />
                <code class="text-md">protected</code>
            </div>
            <div class="flex flex-row align-items-center gap-8">
                <NodeKindIcon
                    nodeKind="Type"
                    typeKind="Class"
                    accessibility={MemberAccessibility.Internal}
                    class="w-48"
                />
                <code class="text-md">internal</code>
            </div>
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Static</strong>
            <span class="text-xs"> When a class or a class member has the static modifier, the icon is filled: </span>
            {#each staticKinds as staticKind}
                <div class="flex flex-row align-items-center gap-8">
                    <NodeKindIcon
                        isStatic={true}
                        nodeKind={staticKind.nodeKind}
                        typeKind={staticKind.typeKind}
                        class="w-32"
                    />
                    <span>Static {staticKind.typeKind ?? staticKind.nodeKind}</span>
                </div>
            {/each}
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Donuts</strong>
            <span class="text-xs"
                >Types have a donut chart surrounding their icon. It can be used to gleam the size of the type as well
                as its ratio of static to instance members.</span
            >
            {@html CHEATSHEET_DONUT}
        </div>
        <div class="flex flex-col gap-8">
            <strong class="extrabold">Abstract and sealed</strong>
            <span class="text-xs">Abstract and sealed types and members have contours surrounding their icon. The sealed modifier creates an uninterrupted octagon, while the abstract modifier forms a partial hexagon:</span>
            {@html CHEATSHEET_CONTOURS}
        </div>
    </div>
</div>
<p>
    Glyphs are nodes of the diagram. The icon in the center represents the kind of the visualized entity. The outlines
    symbolize, in order from innermost to outermost:
</p>
<ul>
    <li>
        <strong>"Staticness"</strong>: If the line is dashed, the entity is static; if it is solid, it is tied to an
        instance.
    </li>
    <li>
        <strong>Instance member count</strong>: The number of instance members inside a type entity. Always solid.
    </li>
    <li>
        <strong>Static member count</strong>: The number of static members inside a type entity. Always dashed.
    </li>
</ul>

<p>
    If a node is on fire, there's an error in the entity it represents. The error is shown in the Properties panel.
    Similarly, if a node is smoldering, there's a warning.
</p>
