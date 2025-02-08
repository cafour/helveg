<script lang="ts">
    import {
        getShortcutHint,
        OP_AUTOLAYOUT,
        OP_COLLAPSE_ALL,
        OP_DIG_IN,
        OP_DIG_OUT,
        OP_EXPAND_ALL,
        OP_GLOBAL_DESELECT,
        OP_INSPECT,
        OP_LAYOUT,
        OP_MOVE,
        OP_REFRESH,
        OP_REMOVE,
        OP_TOGGLE,
        OperationScope,
    } from "../operations";
    import ControlsList from "./ControlsList.svelte";

    const operations = [
        OP_INSPECT,
        OP_TOGGLE,
        OP_MOVE,
        OP_REMOVE,
        OP_LAYOUT,
        OP_AUTOLAYOUT,
        OP_DIG_IN,
        OP_DIG_OUT,
        OP_EXPAND_ALL,
        OP_COLLAPSE_ALL,
        OP_REFRESH,
        OP_GLOBAL_DESELECT,
    ];

    const nodeOperations = operations.filter((o) => (o.scopes & OperationScope.NODE) === OperationScope.NODE);
    const globalOperations = operations.filter((o) => (o.scopes & OperationScope.GLOBAL) === OperationScope.GLOBAL);
</script>

<div class="grid grid-cols-2 gap-16">
    <div>
        <h2 class="extrabold mb-8">Node operations</h2>
        <ControlsList operations={nodeOperations} />
    </div>
    <div>
        <h2 class="extrabold mb-8">Global operations</h2>
        <ControlsList operations={globalOperations} />
    </div>
</div>
