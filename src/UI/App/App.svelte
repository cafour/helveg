<script lang="ts">
  import { AppShell } from "@skeletonlabs/skeleton";
  import { TabGroup, Tab } from "@skeletonlabs/skeleton";
  import Diagram from "Diagram.svelte";
  
  enum TabKind {
    None,
    Properties,
    DocumentInfo
  };

  let currentTab = TabKind.None;
</script>

<AppShell>
  <svelte:fragment slot=pageHeader>
    <div class=p-2>
      <h1>Helveg</h1>
    </div>
  </svelte:fragment>
  <svelte:fragment slot=sidebarRight>
    <TabGroup flex="select-none wm-v"
              regionList="flex-col"
              border="border border-surface-400-500-token"
              active="border-l-4 border-primary-400-500-token"
              padding="px-1 py-3"
              class="flex flex-row-reverse">
      <Tab bind:group={currentTab} name={TabKind.None.toString()} value={TabKind.None} flex=hidden></Tab>
      <Tab bind:group={currentTab} name={TabKind.Properties.toString()} value={TabKind.Properties}>
        Properties
      </Tab>
      <Tab bind:group={currentTab} name={TabKind.DocumentInfo.toString()} value={TabKind.DocumentInfo}>
        Document Info
      </Tab>
      
      <svelte:fragment slot="panel">
        {#if currentTab === TabKind.Properties}
          (properties)
        {:else if currentTab === TabKind.DocumentInfo}
          (document info)
        {/if}
      </svelte:fragment>
    </TabGroup>
  </svelte:fragment>
  
  <Diagram />
</AppShell>
