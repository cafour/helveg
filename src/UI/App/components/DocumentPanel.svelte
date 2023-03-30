<script lang="ts">
    import type { DocumentInfo } from "model/document";

    export let documentInfo: DocumentInfo;
    documentInfo.createdOn = (new Date(documentInfo.createdOn)).toLocaleString();
</script>

<div class="panel flex flex-col overflow-hidden">
    <dl>
        <div class="flex flex-row flex-wrap space-nowrap gap-1 justify-content-between">
            <dt class="bold border-r-1">Name</dt>
            <dd class="flex-grow-1 text-right">{documentInfo.name}</dd>
        </div>
        <div class="flex flex-row flex-wrap space-nowrap gap-1 justify-content-between">
            <dt class="bold border-r-1">Created on</dt>
            <dd class="flex-grow-1 text-right">{documentInfo.createdOn}</dd>
        </div>
        {#if documentInfo.revision}
        <div class="flex flex-row flex-wrap space-nowrap gap-1 justify-content-between">
            <dt class="bold border-r-1">Revision</dt>
            <dd class="flex-grow-1 text-right">{documentInfo.revision}</dd>
        </div>
        {/if}
        <div class="flex flex-row flex-wrap space-nowrap gap-1 justify-content-between">
            <dt class="bold border-r-1">Helveg version</dt>
            <dd class="flex-grow-1 text-right">{documentInfo.helvegVersion}</dd>
        </div>
    </dl>
</div>
