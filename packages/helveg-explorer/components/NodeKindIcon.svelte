<script lang="ts">
    import { getContext, onDestroy } from "svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_NODE_STYLE, MemberAccessibility, type Diagram, type NodeStyle } from "../deps/helveg-diagram";

    export let className: string | undefined = undefined;
    export { className as class };
    export let nodeKind: string;
    export let typeKind: string | undefined = undefined;
    export let methodKind: string | undefined = undefined;
    export let accessibility: MemberAccessibility | undefined = undefined;
    export let isStatic: boolean = false;
    export let isRecord: boolean = false;
    export let isIndexer: boolean = false;
    export let isEnumItem: boolean = false;
    export let isConst: boolean = false;

    const diagram = getContext<Diagram>("diagram");

    function getNodeKindStyle(
        kind: string,
        typeKind?: string,
        methodKind?: string,
        accessibility?: MemberAccessibility,
        isStatic?: boolean,
        isRecord?: boolean,
        isIndexer?: boolean,
        isEnumItem?: boolean,
        isConst?: boolean,
    ): NodeStyle {
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
                    kind,
                    typeKind,
                    methodKind,
                    accessibility,
                    isStatic,
                    isRecord,
                    isIndexer,
                    isEnumItem,
                    isConst,
                },
            },
            diagram.nodeStylistParams,
        );
        return { ...FALLBACK_NODE_STYLE, ...style };
    }

    $: style = getNodeKindStyle(
        nodeKind,
        typeKind,
        methodKind,
        accessibility,
        isStatic,
        isRecord,
        isIndexer,
        isEnumItem,
        isConst,
    );

    const onNodeStylistParamsChanged = () => {
        style = getNodeKindStyle(
            nodeKind,
            typeKind,
            methodKind,
            accessibility,
            isStatic,
            isRecord,
            isIndexer,
            isEnumItem,
            isConst,
        );
    };

    diagram.events.nodeStylistParamsChanged.subscribe(onNodeStylistParamsChanged);

    onDestroy(() => diagram.events.nodeStylistParamsChanged.unsubscribe(onNodeStylistParamsChanged));
</script>

<Icon title={nodeKind} name={style.icon} color={style.color} class={className} />
