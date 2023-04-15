<script lang="ts">
    import type { DocumentInfo } from "model/document";
    import KeyValueList from "./KeyValueList.svelte";

    export let documentInfo: DocumentInfo;
    documentInfo.createdOn = (new Date(documentInfo.createdOn)).toLocaleString();
    
    let items = [
        { key: "Name", value: documentInfo.name },
        { key: "CreatedOn", value: documentInfo.createdOn },
        { key: "Revision", value: documentInfo.revision },
        { key: "HelvegVersion", value: documentInfo.helvegVersion}
    ]
</script>

<div class="panel flex flex-col overflow-hidden">
    <KeyValueList {items} />
</div>
