<script setup lang="ts">
/**
 * Renders the toast queue. Mount once in the layout; drive it with
 * `useToast()` from anywhere.
 */
const { toasts, dismiss } = useToast()

const TONES = {
  success: {
    pill: 'bg-tertiary-container text-on-tertiary',
    icon: 'material-symbols:check-circle-outline'
  },
  error: {
    pill: 'bg-error text-on-error',
    icon: 'material-symbols:error-outline'
  },
  warning: {
    pill: 'bg-amber-500 text-white',
    icon: 'material-symbols:warning-outline'
  },
  info: {
    pill: 'bg-primary-container text-on-primary',
    icon: 'material-symbols:info-outline'
  }
} as const
</script>

<template>
  <div
    class="pointer-events-none fixed bottom-space-lg right-space-lg z-200 flex w-full max-w-sm flex-col gap-space-sm print:hidden"
  >
    <TransitionGroup
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="translate-y-2 opacity-0"
      leave-active-class="transition-all duration-150 ease-in absolute"
      leave-to-class="translate-x-4 opacity-0"
      move-class="transition-transform duration-200"
    >
      <div
        v-for="toast in toasts"
        :key="toast.id"
        role="status"
        class="pointer-events-auto flex items-start gap-space-sm rounded-xl px-space-md py-space-sm font-body-sm text-body-sm elevation-3"
        :class="TONES[toast.tone].pill"
      >
        <Icon :name="TONES[toast.tone].icon" class="mt-0.5 size-5 shrink-0" />
        <p class="flex-1">{{ toast.message }}</p>
        <button
          class="shrink-0 rounded-lg p-0.5 opacity-70 transition-opacity hover:opacity-100"
          aria-label="Dismiss"
          @click="dismiss(toast.id)"
        >
          <Icon name="material-symbols:close" class="size-4" />
        </button>
      </div>
    </TransitionGroup>
  </div>
</template>
