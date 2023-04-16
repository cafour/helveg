<script lang="ts">
    export let name: string;
    export let indent: boolean = true;
</script>

<div class="subpanel flex flex-col">
    <div class="subpanel-header">
        <strong>{name}</strong>
    </div>
    <div class="subpanel-body flex flex-col" class:indent>
        <slot />
    </div>
</div>
