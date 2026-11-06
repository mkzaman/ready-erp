<script setup lang="ts">
/**
 * Horizontal stock-level bar used in the products table.
 *
 * Colour is derived from the value relative to the low-stock threshold so the
 * same rule lives in one place instead of per-page `isLow()` helpers.
 */
const props = withDefaults(
  defineProps<{
    value: number
    /** Bar is full at this value. Defaults to 2x threshold, else the value. */
    max?: number
    threshold?: number | null
    showLabel?: boolean
  }>(),
  {
    showLabel: true
  }
)

const ceiling = computed(() => {
  if (props.max && props.max > 0) return props.max
  if (props.threshold && props.threshold > 0) return props.threshold * 2
  return Math.max(props.value, 1)
})

const percent = computed(() =>
  Math.max(0, Math.min(100, (props.value / ceiling.value) * 100))
)

const state = computed<'out' | 'low' | 'ok'>(() => {
  if (props.value <= 0) return 'out'
  if (props.threshold != null && props.value <= props.threshold) return 'low'
  return 'ok'
})

const FILL = {
  out: 'bg-error',
  low: 'bg-amber-500',
  ok: 'bg-tertiary-container'
} as const

const TEXT = {
  out: 'text-error',
  low: 'text-amber-700',
  ok: 'text-on-surface'
} as const
</script>

<template>
  <div class="flex min-w-32 flex-col gap-1">
    <div v-if="showLabel" class="flex items-baseline justify-between gap-2">
      <span class="font-label-md text-label-md font-semibold tabular" :class="TEXT[state]">
        {{ value }}
      </span>
      <span v-if="threshold != null" class="font-label-sm text-label-sm text-on-surface-variant tabular">
        min {{ threshold }}
      </span>
    </div>

    <div
      class="h-1.5 w-full overflow-hidden rounded-full bg-surface-container"
      role="progressbar"
      :aria-valuenow="value"
      :aria-valuemin="0"
      :aria-valuemax="ceiling"
    >
      <div
        class="h-full rounded-full transition-[width] duration-300"
        :class="FILL[state]"
        :style="{ width: `${percent}%` }"
      />
    </div>
  </div>
</template>
