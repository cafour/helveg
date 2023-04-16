<script lang="ts">
    export let name: string;
</script>

<div class="subpanel flex flex-col">
    <div class="subpanel-header">
        <strong>{name}</strong>
    </div>
    <div class="subpanel-body flex flex-col">
        <slot />
    </div>
</div>
