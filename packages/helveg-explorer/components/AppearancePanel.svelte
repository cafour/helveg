<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppPanels } from "../const.ts";
    import { getContext, onMount } from "svelte";
    import type { Readable, Writable } from "svelte/store";
    import { NodeColorSchemaPreset, type AppearanceOptions } from "../options.ts";
    import {
        getNodeKinds,
        type DataModel,
        cyrb53,
        PizzaIcons,
        getRelations,
        EntityKind,
        LimitedTypeKind,
        UNIVERSAL_NODE_COLOR_SCHEMA,
        VS_NODE_COLOR_SCHEMA,
        TYPE_FOCUS_COLOR_SCHEMA,
        CSHARP_RELATION_HINTS,
        GlyphShape,
        SizingMode,
    } from "../deps/helveg-diagram.ts";
    import Hint from "./Hint.svelte";

    let appearanceOptions = getContext<Writable<AppearanceOptions>>("appearanceOptions");
    $appearanceOptions.relationColors ??= {};
    $appearanceOptions.codePizza.pizzaToppings ??= {};

    let model = getContext<Readable<DataModel>>("model");
    $: pizzaToppings = $appearanceOptions.codePizza.pizzaToppings! ?? {};
    $: appearanceOptions.update((u) => {
        u.codePizza.pizzaToppings = pizzaToppings;
        return u;
    });
    $: relationColors = $appearanceOptions.relationColors! ?? {};
    $: appearanceOptions.update((u) => {
        u.relationColors = relationColors;
        return u;
    });

    $: entityColors = $appearanceOptions.nodeColorSchema.entities;
    function onEntityColorsChanged() {
        appearanceOptions.update((u) => {
            u.nodeColorSchema.entities = entityColors;
            u.nodeColorPreset = NodeColorSchemaPreset.Custom;
            return u;
        });
    }

    $: typeColors = $appearanceOptions.nodeColorSchema.types;
    function onTypeColorsChanged() {
        appearanceOptions.update((u) => {
            u.nodeColorSchema.types = typeColors;
            u.nodeColorPreset = NodeColorSchemaPreset.Custom;
            return u;
        });
    }

    const allToppings = Object.entries(PizzaIcons);

    $: kinds = getNodeKinds($model.data);
    $: relations = getRelations($model.data);
    const entityKinds = Object.values(EntityKind);
    const typeKinds = Object.values(LimitedTypeKind);

    let seed = 42;

    onMount(() => randomizeToppings());

    function randomizeToppings() {
        for (const kind of kinds) {
            const [_, value] = allToppings[cyrb53(kind, seed) % allToppings.length];
            $appearanceOptions.codePizza.pizzaToppings![kind] = value;
        }

        seed++;
    }

    function onNodeColorChanged(preset: string) {
        switch (preset) {
            case NodeColorSchemaPreset.Universal:
                $appearanceOptions.nodeColorSchema = structuredClone(UNIVERSAL_NODE_COLOR_SCHEMA);
                entityColors = $appearanceOptions.nodeColorSchema.entities;
                typeColors = $appearanceOptions.nodeColorSchema.types;
                break;
            case NodeColorSchemaPreset.TypeFocus:
                $appearanceOptions.nodeColorSchema = structuredClone(TYPE_FOCUS_COLOR_SCHEMA);
                entityColors = $appearanceOptions.nodeColorSchema.entities;
                typeColors = $appearanceOptions.nodeColorSchema.types;
                break;
            case NodeColorSchemaPreset.VS:
                $appearanceOptions.nodeColorSchema = structuredClone(VS_NODE_COLOR_SCHEMA);
                entityColors = $appearanceOptions.nodeColorSchema.entities;
                typeColors = $appearanceOptions.nodeColorSchema.types;
                break;
        }
    }

    $: hasDiffMetadata = Object.values($model.data?.nodes ?? {}).some((n) => n.diff !== undefined);
</script>

<Panel name="Appearance" indent={false} id={AppPanels.Appearance}>
    <Subpanel name="Glyphs">
        <label class="flex flex-row gap-8 align-items-center">
            <span class="w-160 inline-block flex-shrink-0 ellipsis overflow-hidden" title="Shape"
                >Shape
                <Hint
                    text="The basic shape of glyphs. `None` only renders icons, `Outlines` is a legacy shape, and `Donut` is the default."
                />
            </span>
            <select bind:value={$appearanceOptions.glyph.glyphShape}>
                {#each Object.values(GlyphShape) as shape}
                    <option value={shape}>{shape}</option>
                {/each}
            </select>
        </label>
        <label class="flex flex-row gap-8 align-items-center">
            <span class="w-160 inline-block flex-shrink-0 ellipsis overflow-hidden" title="Sizing mode"
                >Sizing mode
                <Hint
                    text="The method for determining node size. `Linear` is the default, `Sqrt` takes the square root of the size value, and `Log` takes the log of that value."
                />
            </span>
            <select bind:value={$appearanceOptions.glyph.sizingMode}>
                {#each Object.values(SizingMode) as mode}
                    <option value={mode}>{mode}</option>
                {/each}
            </select>
        </label>
        <label class="flex flex-row gap-8 align-items-center">
            <span class="w-160 inline-block flex-shrink-0 ellipsis overflow-hidden" title="Node color preset"
                >Node color preset
                <Hint
                    text="The palette applied to the nodes. `Universal` is default and is usable in most situations. `TypeFocus` applies colors to type kinds, graying out most else, and lets you focus on types only. `VS` is a legacy palette based on Visual Studio. `Custom` is automatically selected when you manually change a color in the `Entity colors` panel below."
                />
            </span>
            <select
                on:change={(e) => onNodeColorChanged(e.currentTarget.value)}
                bind:value={$appearanceOptions.nodeColorPreset}
            >
                {#each Object.values(NodeColorSchemaPreset) as schemaPreset}
                    <option value={schemaPreset} disabled={schemaPreset == NodeColorSchemaPreset.Custom}
                        >{schemaPreset}</option
                    >
                {/each}
            </select>
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showIcons} />
            Icons
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showLabels} />
            Labels
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showFire} />
            Fire
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.isFireAnimated} />
            Fire animation
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showCollapsedNodeIndicators} />
            Collapsed node indicators
            <Hint text="Renders little crescent shapes beneath nodes that may be expanded." />
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showHatching} />
            Hatching
            <Hint text="Renders a hatching effect over the type nodes' sector that represent non-static members." />
        </label>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.glyph.showContours} />
            Contours
            <Hint text="Outlines around icons that represent certain modifiers." />
        </label>
        {#if hasDiffMetadata}
            <label>
                <input type="checkbox" bind:checked={$appearanceOptions.glyph.showDiffs} />
                Diffs
            </label>
        {/if}
    </Subpanel>
    <Subpanel name="Relation colors">
        {#each relations as relation}
            <label class="flex flex-row gap-8 align-items-center">
                <span class="inline-block flex-grow-1">{relation}</span>
                {#if CSHARP_RELATION_HINTS[relation] != null}
                    <Hint text={CSHARP_RELATION_HINTS[relation]} />
                {/if}
                <input
                    type="color"
                    value={relationColors[relation]}
                    on:change={(e) => (relationColors[relation] = e.currentTarget.value)}
                />
            </label>
        {/each}
    </Subpanel>
    <Subpanel name="Entity colors" hint="Node colors applied based on the `kind` property.">
        <div class="flex flex-row gap-8 align-items-center">
            <strong class="flex-grow-1">&nbsp;</strong>
            <strong class="w-48">Fg <Hint text="Foreground color" /></strong>
            <strong class="w-48">Bg <Hint text="Background color" /></strong>
        </div>
        {#each entityKinds as entityKind}
            <div class="flex flex-row gap-8 align-items-center">
                <span class="inline-block flex-grow-1 overflow-hidden ellipsis" title={entityKind}>{entityKind}</span>
                <input
                    class="w-48 flex-shrink-0"
                    type="color"
                    value={entityColors[entityKind].foreground}
                    on:change={(e) => {
                        entityColors[entityKind].foreground = e.currentTarget.value;
                        onEntityColorsChanged();
                    }}
                />
                <input
                    class="w-48 flex-shrink-0"
                    type="color"
                    value={entityColors[entityKind].background}
                    on:change={(e) => {
                        entityColors[entityKind].background = e.currentTarget.value;
                        onEntityColorsChanged();
                    }}
                />
            </div>
        {/each}
    </Subpanel>
    <Subpanel
        name="Type colors"
        hint="Node colors applied based on the `typeKind` property. Overrides `Entity colors` above."
    >
        <div class="flex flex-row gap-8 align-items-center">
            <strong class="flex-grow-1">&nbsp;</strong>
            <strong class="w-48">Fg <Hint text="Foreground color" /></strong>
            <strong class="w-48">Bg <Hint text="Background color" /></strong>
        </div>
        {#each typeKinds as typeKind}
            <div class="flex flex-row gap-8 align-items-center">
                <span class="inline-block flex-grow-1 overflow-hidden ellipsis" title={typeKind}>{typeKind}</span>
                <input
                    class="w-48 flex-shrink-0"
                    type="color"
                    value={typeColors[typeKind].foreground}
                    on:change={(e) => {
                        typeColors[typeKind].foreground = e.currentTarget.value;
                        onTypeColorsChanged();
                    }}
                />
                <input
                    class="w-48 flex-shrink-0"
                    type="color"
                    value={typeColors[typeKind].background}
                    on:change={(e) => {
                        typeColors[typeKind].background = e.currentTarget.value;
                        onTypeColorsChanged();
                    }}
                />
            </div>
        {/each}
    </Subpanel>
    <Subpanel name="CodePizza" collapsed={true}>
        <label>
            <input type="checkbox" bind:checked={$appearanceOptions.codePizza.isEnabled} />
            Enabled
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            CrustWidth
            <input type="number" min="1" bind:value={$appearanceOptions.codePizza.crustWidth} />
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            SauceWidth
            <input type="number" min="1" bind:value={$appearanceOptions.codePizza.sauceWidth} />
        </label>

        <hr />

        <button class="button-stretch mb-16" on:click={() => randomizeToppings()}>Randomize</button>

        {#each kinds as kind}
            <label class="flex flex-row gap-8 align-items-center">
                <span class="w-80 inline-block flex-shrink-0 ellipsis overflow-hidden" title={kind}>{kind}</span>
                <select bind:value={pizzaToppings[kind]}>
                    {#each allToppings as topping}
                        <option value={topping[1]}>{topping[0]}</option>
                    {/each}
                </select>
            </label>
        {/each}
    </Subpanel>
</Panel>
