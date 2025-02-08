<script lang="ts">
    import { getContext, onDestroy } from "svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_NODE_STYLE, type Diagram, type NodeStyle } from "../deps/helveg-diagram";

    export let nodeKind: string;

    const diagram = getContext<Diagram>("diagram");

    function getNodeKindStyle(kind: string): NodeStyle {
        const style = diagram.nodeStylist(
            {
                id: "<invalid>",
                x: 0,
                y: 0,
                highlighted: false,
                hidden: false,
                label: "<invalid>",
                size: 1,
                color: "#000000",
                forceLabel: false,
                zIndex: 0,
                type: "glyph",
                model: {
                    kind: kind,
                },
            },
            diagram.nodeStylistParams,
        );
        return { ...FALLBACK_NODE_STYLE, ...style };
    }

    $: style = getNodeKindStyle(nodeKind);

    const onNodeStylistChanged = () => {
        style = getNodeKindStyle(nodeKind);
    };

    diagram.events.nodeStylistChanged.subscribe(onNodeStylistChanged);

    onDestroy(() => diagram.events.nodeStylistChanged.unsubscribe(onNodeStylistChanged));
</script>

<Icon title={nodeKind} name={style.icon} color={style.color} />
