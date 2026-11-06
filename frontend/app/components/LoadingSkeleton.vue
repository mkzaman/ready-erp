<script setup lang="ts">
/**
 * Shimmer placeholder. Use `rows` + `columns` to approximate a table body
 * while data is in flight.
 */
withDefaults(
  defineProps<{
    rows?: number
    columns?: number
    /** Render a single block instead of a grid. */
    block?: boolean
    height?: string
  }>(),
  {
    rows: 5,
    columns: 1,
    height: 'h-4'
  }
)
</script>

<template>
  <div v-if="block" class="animate-pulse rounded-xl bg-surface-container" :class="height" />

  <div v-else class="flex flex-col gap-space-sm" aria-busy="true" aria-live="polite">
    <div
      v-for="row in rows"
      :key="row"
      class="grid gap-space-md"
      :style="{ gridTemplateColumns: `repeat(${columns}, minmax(0, 1fr))` }"
    >
      <div
        v-for="col in columns"
        :key="col"
        class="animate-pulse rounded-lg bg-surface-container"
        :class="height"
      />
    </div>
  </div>
</template>
