<script setup lang="ts">
/**
 * Authenticated application shell: fixed sidebar + topbar, scrollable content.
 *
 * Below `lg` the sidebar becomes an off-canvas drawer and the content loses
 * its left offset. All chrome is hidden when printing so document views
 * (invoices, dispatch notes) occupy the full page.
 */
const navOpen = ref(false)
</script>

<template>
  <div class="min-h-screen bg-background font-body-md text-body-md text-on-surface">
    <AppSidebar v-model:open="navOpen" />
    <AppTopbar v-model:nav-open="navOpen" />

    <div class="flex min-h-screen flex-col lg:pl-64 print:pl-0">
      <main
        class="flex-1 px-space-md pb-space-xl pt-[calc(4rem+var(--spacing-space-md))] lg:px-space-xl lg:pt-[calc(4rem+var(--spacing-space-xl))] print:p-0 print:pt-0"
      >
        <slot />
      </main>
    </div>

    <ToastHost />
  </div>
</template>
