<script lang="ts">
    import { getContext } from "svelte";
    import Icon from "./Icon.svelte";
    import { FALLBACK_NODE_STYLE, type Diagram, type NodeStyle } from "../deps/helveg-diagram";

    export let nodeKind: string;
    
    const diagram = getContext<Diagram>("diagram");

    function getNodeKindStyle(kind: string): NodeStyle {
        const style = diagram.nodeStylist({
            kind: kind,
        });
        return {...FALLBACK_NODE_STYLE, ...style};
    }
    
    $: style = getNodeKindStyle(nodeKind);
</script>

<Icon title={nodeKind} name={style.icon} color={style.color} />
