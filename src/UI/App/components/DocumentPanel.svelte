<script lang="ts">
    import type { DocumentInfo } from "model/document";
    import KeyValueList from "./KeyValueList.svelte";
    import Panel from "./Panel.svelte";

    export let documentInfo: DocumentInfo;

    $: items = [
        { key: "Name", value: documentInfo.name },
        { key: "CreatedOn", value: new Date(documentInfo.createdOn).toLocaleString() },
        { key: "Revision", value: documentInfo.revision },
        { key: "HelvegVersion", value: documentInfo.helvegVersion },
    ];
</script>

<Panel name="Document">
    <KeyValueList {items} />
</Panel>
