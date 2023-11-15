<script lang="ts">
    import Panel from "./Panel.svelte";
    import Subpanel from "./Subpanel.svelte";
    import { AppPanels } from "../const.ts";
    import { getContext, onMount } from "svelte";
    import type { Readable, Writable } from "svelte/store";
    import type { AppearanceOptions } from "../options.ts";
    import {
        getNodeKinds,
        type DataModel,
        cyrb53,
        PizzaIcons,
        getRelations,
    } from "../deps/helveg-diagram.ts";

    let appearanceOptions =
        getContext<Writable<AppearanceOptions>>("appearanceOptions");

    let model = getContext<Readable<DataModel>>("model");
    let toppings: Record<string, PizzaIcons> =
        $appearanceOptions.codePizza.pizzaToppings ?? {};
    let relationColors: Record<string, string> = 
        $appearanceOptions.relationColors ?? {};
    $: $appearanceOptions.relationColors = relationColors;

    const allToppings = Object.entries(PizzaIcons);

    $: kinds = getNodeKinds($model.data);
    $: relations = getRelations($model.data);
    $: toppings = randomizeToppings(kinds);
    $: $appearanceOptions.codePizza.pizzaToppings = toppings;

    let seed = 42;

    function randomizeToppings(nodeKinds?: string[]) {
        nodeKinds ??= kinds;
        let toppings: Record<string, PizzaIcons> = {};
        for (const kind of nodeKinds) {
            const [_, value] =
                allToppings[cyrb53(kind, seed) % allToppings.length];
            toppings[kind] = toppings[kind] ?? value;
        }

        seed++;
        return toppings;
    }
</script>

<Panel name="Appearance" indent={false} id={AppPanels.Appearance}>
    <Subpanel name="Glyphs">
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.glyph.showIcons}
            />
            ShowIcons
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.glyph.showOutlines}
            />
            ShowOutlines
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.glyph.showLabels}
            />
            ShowLabels
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.glyph.showFire}
            />
            ShowFire
        </label>
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.glyph.isFireAnimated}
            />
            IsFireAnimated
        </label>
    </Subpanel>
    <Subpanel name="Relations">
        {#each relations as relation}
            <label class="flex flex-row gap-8 align-items-center">
                <span class="inline-block flex-grow-1">{relation}</span>
                <input type="color" bind:value={relationColors[relation]} />
            </label>
        {/each}
    </Subpanel>
    <Subpanel name="CodePizza">
        <label>
            <input
                type="checkbox"
                bind:checked={$appearanceOptions.codePizza.isEnabled}
            />
            Enabled
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            CrustWidth
            <input
                type="number"
                min="1"
                bind:value={$appearanceOptions.codePizza.crustWidth}
            />
        </label>

        <label class="flex flex-row gap-8 align-items-center">
            SauceWidth
            <input
                type="number"
                min="1"
                bind:value={$appearanceOptions.codePizza.sauceWidth}
            />
        </label>

        <hr />

        <button
            class="button-stretch mb-16"
            on:click={() => (toppings = randomizeToppings(kinds))}
            >Randomize</button
        >

        {#each kinds as kind}
            <label class="flex flex-row gap-8 align-items-center">
                <span
                    class="w-80 inline-block flex-shrink-0 ellipsis overflow-hidden"
                    title={kind}>{kind}</span
                >
                <select bind:value={toppings[kind]}>
                    {#each allToppings as topping}
                        <option value={topping[1]}>{topping[0]}</option>
                    {/each}
                </select>
            </label>
        {/each}
    </Subpanel>
</Panel>
