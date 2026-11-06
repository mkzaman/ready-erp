<script setup lang="ts">
/**
 * Metric tile for dashboard / list-page KPI rows.
 *
 * Supports an optional trend delta and an optional inline sparkline rendered
 * from a plain number series (no charting dependency).
 */
const props = withDefaults(
  defineProps<{
    label: string
    value: string | number
    /** Material Symbols icon name shown in the top-right tile. */
    icon?: string
    /** Signed change, e.g. "+12.4%". Colour is driven by `trend`. */
    delta?: string
    trend?: 'up' | 'down' | 'flat'
    /** Secondary line under the value. */
    caption?: string
    /** Values for the inline sparkline. Needs at least 2 points. */
    series?: number[]
    /** Tints the value when the metric represents a problem (e.g. low stock). */
    tone?: 'default' | 'warning' | 'danger'
    loading?: boolean
  }>(),
  {
    trend: 'flat',
    tone: 'default'
  }
)

const TREND_CLASS = {
  up: 'text-tertiary-container',
  down: 'text-error',
  flat: 'text-on-surface-variant'
} as const

const TREND_ICON = {
  up: 'material-symbols:trending-up',
  down: 'material-symbols:trending-down',
  flat: 'material-symbols:trending-flat'
} as const

const VALUE_CLASS = {
  default: 'text-on-surface',
  warning: 'text-amber-600',
  danger: 'text-error'
} as const

/** Maps the series into a 100x32 viewBox polyline. */
const sparklinePoints = computed(() => {
  const data = props.series
  if (!data || data.length < 2) return null

  const min = Math.min(...data)
  const max = Math.max(...data)
  const span = max - min || 1

  return data
    .map((n, i) => {
      const x = (i / (data.length - 1)) * 100
      const y = 32 - ((n - min) / span) * 28 - 2
      return `${x.toFixed(2)},${y.toFixed(2)}`
    })
    .join(' ')
})
</script>

<template>
  <div
    class="group relative flex flex-col justify-between overflow-hidden rounded-xl bg-surface-container-lowest p-space-lg elevation-1"
  >
    <div class="flex items-start justify-between gap-space-md">
      <span class="font-label-md text-label-md uppercase tracking-wider text-on-surface-variant">
        {{ label }}
      </span>
      <span
        v-if="icon"
        class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-surface-container text-primary"
      >
        <Icon :name="`material-symbols:${icon}`" class="size-5" />
      </span>
    </div>

    <div class="mt-space-sm">
      <div
        v-if="loading"
        class="h-9 w-28 animate-pulse rounded-lg bg-surface-container"
      />
      <p v-else class="font-metric-val text-metric-val tabular" :class="VALUE_CLASS[tone]">
        {{ value }}
      </p>
      <p v-if="caption" class="mt-0.5 font-body-sm text-body-sm text-on-surface-variant">
        {{ caption }}
      </p>
    </div>

    <svg
      v-if="sparklinePoints"
      class="mt-space-sm h-8 w-full text-primary-container"
      viewBox="0 0 100 32"
      preserveAspectRatio="none"
      aria-hidden="true"
    >
      <polyline
        :points="sparklinePoints"
        fill="none"
        stroke="currentColor"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
        vector-effect="non-scaling-stroke"
      />
    </svg>

    <div
      v-if="delta || $slots.footer"
      class="mt-space-md flex items-center justify-between gap-space-sm border-t border-surface-container pt-space-sm font-label-sm text-label-sm text-on-surface-variant"
    >
      <span v-if="delta" class="flex items-center gap-1 font-semibold" :class="TREND_CLASS[trend]">
        <Icon :name="TREND_ICON[trend]" class="size-4" />
        {{ delta }}
      </span>
      <slot name="footer" />
    </div>
  </div>
</template>
