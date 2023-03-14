<script lang="ts">
  import Counter from "./Counter.svelte";
  import { AppShell } from '@skeletonlabs/skeleton';
  import { FileButton } from '@skeletonlabs/skeleton';
</script>

<AppShell>
  <svelte:fragment slot="header">
    <h1>Helveg</h1>
  
    <Counter />
    <FileButton name="files" button="variant-soft-primary"/>
  </svelte:fragment>
</AppShell>
