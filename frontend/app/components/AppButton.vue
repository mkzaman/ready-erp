<script setup lang="ts">
/**
 * Primary action control for the StockPulse design system.
 *
 * Renders a real <button>, or a <NuxtLink> when `to` is supplied, so
 * navigational actions keep correct semantics (href, middle-click, open in
 * new tab) without duplicating styles.
 *
 * The two branches are written out explicitly rather than via
 * `<component :is="resolveComponent('NuxtLink')">`: that indirection resolves
 * at runtime and silently renders an inert `<nuxtlink>` element if resolution
 * fails. The explicit tag is resolved at compile time by Nuxt's auto-import.
 */
type Variant = 'primary' | 'secondary' | 'ghost' | 'destructive'
type Size = 'sm' | 'md' | 'icon'

const props = withDefaults(
  defineProps<{
    variant?: Variant
    size?: Size
    /** Material Symbols icon name, e.g. 'add' or 'download'. */
    icon?: string
    /** Renders the icon after the label instead of before it. */
    iconTrailing?: boolean
    disabled?: boolean
    loading?: boolean
    type?: 'button' | 'submit' | 'reset'
    /** When set, renders a NuxtLink instead of a button. */
    to?: string
  }>(),
  {
    variant: 'primary',
    size: 'md',
    type: 'button'
  }
)

const BASE =
  'inline-flex cursor-pointer items-center justify-center gap-space-xs font-label-lg text-label-lg ' +
  'transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-container/40 ' +
  'focus-visible:ring-offset-2 focus-visible:ring-offset-background active:scale-[0.98] ' +
  'disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-40'

const VARIANTS: Record<Variant, string> = {
  primary: 'rounded-xl bg-primary-container text-on-primary shadow-md hover:bg-primary',
  secondary:
    'rounded-xl bg-surface-container-lowest text-on-surface shadow-sm hover:bg-surface-container-low',
  ghost: 'rounded-lg text-on-surface-variant hover:text-primary hover:bg-surface-container',
  destructive: 'rounded-xl bg-error-container text-on-error-container hover:brightness-95'
}

const SIZES: Record<Size, string> = {
  sm: 'px-space-sm py-1.5 text-label-sm font-label-sm',
  md: 'px-space-md py-2',
  icon: 'p-1.5'
}

const classes = computed(() => [BASE, VARIANTS[props.variant], SIZES[props.size]])

const iconName = computed(() => (props.icon ? `material-symbols:${props.icon}` : null))
const isDisabled = computed(() => props.disabled || props.loading)
</script>

<template>
  <NuxtLink v-if="to" :to="to" :class="classes">
    <Icon v-if="iconName && !iconTrailing" :name="iconName" class="size-[18px] shrink-0" />
    <slot />
    <Icon v-if="iconName && iconTrailing" :name="iconName" class="size-[18px] shrink-0" />
  </NuxtLink>

  <button
    v-else
    :type="type"
    :disabled="isDisabled"
    :aria-busy="loading || undefined"
    :class="classes"
  >
    <Icon
      v-if="loading"
      name="material-symbols:progress-activity"
      class="size-[18px] shrink-0 animate-spin"
    />
    <Icon
      v-else-if="iconName && !iconTrailing"
      :name="iconName"
      class="size-[18px] shrink-0"
    />

    <slot />

    <Icon
      v-if="iconName && iconTrailing && !loading"
      :name="iconName"
      class="size-[18px] shrink-0"
    />
  </button>
</template>
