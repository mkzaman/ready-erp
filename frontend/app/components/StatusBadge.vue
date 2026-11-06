<script setup lang="ts">
/**
 * Pill status indicator used by inventory tables (stock state) and document
 * tables (quotation / invoice / dispatch state).
 *
 * Pass a known `status` string for automatic colouring, or override with
 * `tone` for anything unmapped.
 */
type Tone = 'success' | 'warning' | 'danger' | 'info' | 'neutral'

const props = withDefaults(
  defineProps<{
    status: string
    tone?: Tone
    /** Replaces the leading dot with a Material Symbols icon. */
    icon?: string
    /** Hide the leading dot entirely (used for category tags). */
    plain?: boolean
  }>(),
  {}
)

const TONES: Record<Tone, { pill: string; dot: string }> = {
  success: { pill: 'bg-tertiary-container/15 text-tertiary-container', dot: 'bg-tertiary-container' },
  warning: { pill: 'bg-amber-50 text-amber-700', dot: 'bg-amber-500' },
  danger: { pill: 'bg-error-container text-on-error-container', dot: 'bg-error' },
  info: { pill: 'bg-surface-container text-primary', dot: 'bg-primary' },
  neutral: { pill: 'bg-surface-container text-on-surface-variant', dot: 'bg-on-surface-variant' }
}

/** Normalised status -> tone. Keys are lowercased and stripped of spaces. */
const STATUS_TONES: Record<string, Tone> = {
  // Inventory
  instock: 'success',
  lowstock: 'warning',
  outofstock: 'danger',
  intransit: 'info',
  // Documents
  draft: 'neutral',
  sent: 'info',
  accepted: 'success',
  approved: 'success',
  issued: 'success',
  paid: 'success',
  converted: 'info',
  pending: 'warning',
  overdue: 'danger',
  rejected: 'danger',
  cancelled: 'danger'
}

const tone = computed<Tone>(() => {
  if (props.tone) return props.tone
  const key = props.status?.toLowerCase().replace(/[\s_-]/g, '') ?? ''
  return STATUS_TONES[key] ?? 'neutral'
})

const styles = computed(() => TONES[tone.value])
</script>

<template>
  <span
    class="inline-flex items-center gap-1 whitespace-nowrap rounded-full px-2 py-0.5 font-label-sm text-label-sm font-semibold"
    :class="styles.pill"
  >
    <Icon v-if="icon" :name="`material-symbols:${icon}`" class="size-3.5 shrink-0" />
    <span v-else-if="!plain" class="size-1.5 shrink-0 rounded-full" :class="styles.dot" />
    {{ status }}
  </span>
</template>
