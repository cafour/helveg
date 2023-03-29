<script lang="ts">
    // import { AppShell } from "@skeletonlabs/skeleton";
    // import { TabGroup, Tab } from "@skeletonlabs/skeleton";
    // import Diagram from "./Diagram.svelte";
    import Dock from "./Dock.svelte";
    import Tab from "./Tab.svelte";
</script>

<Dock>
    <Tab name="Data" value=data-panel>
        <strong>TODO: Data Panel Contents</strong>
    </Tab>
    <Tab name="Layout" value=layout-panel>
        <strong>TODO: Layout Panel Contents</strong>
    </Tab>
    <Tab name="Glyphs" value=glyphs-panel>
        <strong>TODO: Glyphs Panel Contents</strong>
    </Tab>
    <Tab name="Properties" value=properties-panel>
        <strong>TODO: Properties Panel Contents</strong>
    </Tab>
    <Tab name="Document" value=document-panel>
        <strong>TODO: Document Panel Contents</strong>
    </Tab>
</Dock>

<!-- <Diagram /> -->

<!-- <AppShell>
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
</AppShell> -->
