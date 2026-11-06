<script setup lang="ts">
import type { TableTab } from '~/types/table'

/**
 * Filter bar that sits directly above <DataTable>. Visually fused to the table
 * via `rounded-t-xl`, matching the StockPulse layout.
 */
withDefaults(
  defineProps<{
    searchPlaceholder?: string
    /** Optional pill-tab filter group. */
    tabs?: TableTab[]
    /** Number of currently selected rows; shows the bulk-action area. */
    selectedCount?: number
  }>(),
  {
    searchPlaceholder: 'Search...',
    selectedCount: 0
  }
)

const search = defineModel<string>('search', { default: '' })
const tab = defineModel<string>('tab', { default: '' })
</script>

<template>
  <div
    class="flex flex-col justify-between gap-space-md rounded-t-xl bg-surface-container-lowest p-space-md elevation-1 lg:flex-row lg:items-center"
  >
    <div class="relative w-full lg:w-96">
      <Icon
        name="material-symbols:search"
        class="pointer-events-none absolute left-3 top-1/2 size-5 -translate-y-1/2 text-on-surface-variant"
      />
      <input
        v-model="search"
        type="search"
        :placeholder="searchPlaceholder"
        class="h-10 w-full rounded-xl bg-surface-container-low pl-10 pr-4 font-body-sm text-body-sm text-on-surface transition-all placeholder:text-on-surface-variant focus:bg-surface-container-lowest focus:shadow-[0_0_0_2px_rgb(37_99_235_/_0.4)] focus:outline-none"
      >
    </div>

    <div class="flex flex-wrap items-center gap-space-sm">
      <div
        v-if="tabs?.length"
        class="flex items-center rounded-xl bg-surface-container-low p-0.5"
        role="tablist"
      >
        <button
          v-for="t in tabs"
          :key="t.value"
          type="button"
          role="tab"
          :aria-selected="tab === t.value"
          class="flex items-center gap-1.5 rounded-lg px-3 py-1.5 font-label-sm text-label-sm transition-all"
          :class="
            tab === t.value
              ? 'bg-surface-container-lowest text-primary elevation-1'
              : 'text-on-surface-variant hover:text-on-surface'
          "
          @click="tab = t.value"
        >
          <Icon v-if="t.icon" :name="`material-symbols:${t.icon}`" class="size-4" />
          {{ t.label }}
          <span v-if="t.count != null" class="tabular opacity-70">({{ t.count }})</span>
        </button>
      </div>

      <slot name="filters" />

      <Transition
        enter-active-class="transition-all duration-150"
        enter-from-class="opacity-0 scale-95"
        leave-active-class="transition-all duration-150"
        leave-to-class="opacity-0 scale-95"
      >
        <div v-if="selectedCount > 0" class="flex items-center gap-space-sm">
          <span class="font-label-md text-label-md text-on-surface-variant">
            {{ selectedCount }} selected
          </span>
          <slot name="bulk" />
        </div>
      </Transition>
    </div>
  </div>
</template>
