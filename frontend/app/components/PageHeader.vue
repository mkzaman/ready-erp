<script setup lang="ts">
/**
 * Page title block: heading, optional status pill, supporting copy and a
 * right-aligned actions area.
 */
defineProps<{
  title: string
  description?: string
  /** Small pill beside the title, e.g. "Live Catalog". */
  badge?: string
}>()
</script>

<template>
  <div
    class="mb-space-xl flex flex-col gap-space-md md:flex-row md:items-center md:justify-between"
  >
    <div>
      <div class="flex items-center gap-space-sm">
        <h1
          class="font-display-lg-mobile text-display-lg-mobile text-on-surface md:font-display-lg md:text-display-lg"
        >
          {{ title }}
        </h1>
        <span
          v-if="badge"
          class="rounded-full bg-tertiary-container/15 px-2.5 py-1 font-label-sm text-label-sm text-tertiary-container"
        >
          {{ badge }}
        </span>
      </div>
      <p v-if="description" class="mt-1 font-body-md text-body-md text-on-surface-variant">
        {{ description }}
      </p>
    </div>

    <div v-if="$slots.actions" class="flex shrink-0 flex-wrap items-center gap-space-sm">
      <slot name="actions" />
    </div>
  </div>
</template>
