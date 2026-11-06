<script setup lang="ts">
/**
 * Dialog / slide-over. Replaces the four duplicated
 * `fixed inset-0 bg-black/40` overlays.
 *
 * Variants:
 *   - `dialog` (default) centred modal
 *   - `drawer`           right-hand slide-over, used for long record forms
 *
 * Set `printable` for the invoice / dispatch document views: the chrome is
 * hidden and the panel flows as normal page content when printing.
 */
const props = withDefaults(
  defineProps<{
    open: boolean
    title?: string
    description?: string
    variant?: 'dialog' | 'drawer'
    size?: 'sm' | 'md' | 'lg' | 'xl'
    printable?: boolean
    /** Disable closing via backdrop click / Escape. */
    persistent?: boolean
  }>(),
  {
    variant: 'dialog',
    size: 'md'
  }
)

const emit = defineEmits<{ close: [] }>()

const SIZES = {
  sm: 'max-w-md',
  md: 'max-w-2xl',
  lg: 'max-w-4xl',
  xl: 'max-w-6xl'
} as const

const titleId = useId()

function requestClose() {
  if (!props.persistent) emit('close')
}

function onKeydown(event: KeyboardEvent) {
  if (event.key === 'Escape') requestClose()
}

// Prevent the page behind the overlay from scrolling while it is open.
watch(
  () => props.open,
  (open) => {
    if (typeof document === 'undefined') return
    document.body.style.overflow = open ? 'hidden' : ''
    if (open) document.addEventListener('keydown', onKeydown)
    else document.removeEventListener('keydown', onKeydown)
  },
  { immediate: true }
)

onBeforeUnmount(() => {
  if (typeof document === 'undefined') return
  document.body.style.overflow = ''
  document.removeEventListener('keydown', onKeydown)
})
</script>

<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition-opacity duration-150"
      leave-active-class="transition-opacity duration-150"
      enter-from-class="opacity-0"
      leave-to-class="opacity-0"
    >
      <div
        v-if="open"
        class="fixed inset-0 z-100 flex bg-on-background/40 backdrop-blur-[4px]"
        :class="[
          variant === 'drawer' ? 'justify-end' : 'items-center justify-center p-space-md',
          printable ? 'print:static print:block print:bg-transparent print:backdrop-blur-none' : ''
        ]"
        role="dialog"
        aria-modal="true"
        :aria-labelledby="title ? titleId : undefined"
        @click.self="requestClose"
      >
        <Transition
          appear
          :enter-active-class="`transition-all duration-200 ease-out`"
          :enter-from-class="variant === 'drawer' ? 'translate-x-full' : 'opacity-0 scale-95'"
          :leave-active-class="`transition-all duration-150 ease-in`"
          :leave-to-class="variant === 'drawer' ? 'translate-x-full' : 'opacity-0 scale-95'"
        >
          <div
            class="flex flex-col bg-surface-container-lowest elevation-3"
            :class="[
              variant === 'drawer'
                ? 'h-full w-full max-w-xl'
                : `w-full ${SIZES[size]} max-h-[90vh] rounded-xl`,
              printable ? 'print:max-h-none print:w-full print:max-w-none print:rounded-none print:shadow-none' : ''
            ]"
          >
            <header
              v-if="title || $slots.header"
              class="flex shrink-0 items-start justify-between gap-space-md border-b border-surface-container px-space-lg py-space-md"
              :class="printable ? 'print:hidden' : ''"
            >
              <slot name="header">
                <div>
                  <h2 :id="titleId" class="font-headline-sm text-headline-sm text-on-surface">
                    {{ title }}
                  </h2>
                  <p
                    v-if="description"
                    class="mt-0.5 font-body-sm text-body-sm text-on-surface-variant"
                  >
                    {{ description }}
                  </p>
                </div>
              </slot>

              <AppButton
                variant="ghost"
                size="icon"
                icon="close"
                aria-label="Close"
                @click="requestClose"
              />
            </header>

            <div
              class="flex-1 overflow-y-auto px-space-lg py-space-md"
              :class="printable ? 'print:overflow-visible print:p-0' : ''"
            >
              <slot />
            </div>

            <footer
              v-if="$slots.footer"
              class="flex shrink-0 items-center justify-end gap-space-sm border-t border-surface-container px-space-lg py-space-md"
              :class="printable ? 'print:hidden' : ''"
            >
              <slot name="footer" />
            </footer>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>
