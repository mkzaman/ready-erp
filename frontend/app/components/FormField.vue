<script setup lang="ts">
/**
 * Labelled form control. Replaces the ~22 hand-rolled
 * `<label class="flex flex-col text-xs ...">` blocks duplicated across pages.
 *
 * Renders an <input>, <select> (pass <option>s via the default slot) or
 * <textarea> depending on `as`.
 */
const props = withDefaults(
  defineProps<{
    label?: string
    as?: 'input' | 'select' | 'textarea'
    type?: string
    placeholder?: string
    hint?: string
    error?: string
    required?: boolean
    disabled?: boolean
    /** Coerce the bound value to a number (replaces v-model.number). */
    numeric?: boolean
    rows?: number
    /** Leading Material Symbols icon name, inputs only. */
    icon?: string
  }>(),
  {
    as: 'input',
    type: 'text',
    rows: 3
  }
)

// Forward stray attributes (autocomplete, min, max, step, name, ...) to the
// control itself rather than letting them land on the wrapper <div>.
defineOptions({ inheritAttrs: false })

const model = defineModel<string | number | null>()

const id = useId()

const CONTROL =
  'w-full rounded-xl bg-surface-container-low text-on-surface font-body-sm text-body-sm ' +
  'placeholder:text-on-surface-variant transition-all focus:outline-none ' +
  'focus:bg-surface-container-lowest disabled:opacity-50 disabled:pointer-events-none'

const controlClass = computed(() => [
  CONTROL,
  props.as === 'textarea' ? 'py-2.5 px-3' : 'h-10 px-3',
  props.icon && props.as === 'input' ? 'pl-10' : '',
  props.error
    ? 'shadow-[0_0_0_2px_var(--color-error)]'
    : 'focus:shadow-[0_0_0_2px_rgb(37_99_235_/_0.4)]',
  props.as === 'select' ? 'appearance-none pr-9 cursor-pointer' : ''
])

function onInput(event: Event) {
  const raw = (event.target as HTMLInputElement | HTMLTextAreaElement).value
  if (!props.numeric) {
    model.value = raw
    return
  }
  // Keep an empty field as null rather than NaN or 0, so "no value" is
  // distinguishable from "zero" (matters for lowStockThreshold).
  model.value = raw === '' ? null : Number(raw)
}
</script>

<template>
  <div class="flex flex-col gap-1.5">
    <label
      v-if="label"
      :for="id"
      class="font-label-md text-label-md text-on-surface-variant"
    >
      {{ label }}
      <span v-if="required" class="text-error">*</span>
    </label>

    <div class="relative">
      <Icon
        v-if="icon && as === 'input'"
        :name="`material-symbols:${icon}`"
        class="pointer-events-none absolute left-3 top-1/2 size-5 -translate-y-1/2 text-on-surface-variant"
      />

      <!--
        Selects bind with v-model rather than :value/@change so that non-string
        option values (numeric ids, null) survive. Reading event.target.value
        would stringify them.
      -->
      <select
        v-if="as === 'select'"
        :id="id"
        v-model="model"
        :disabled="disabled"
        :required="required"
        :class="controlClass"
        :aria-invalid="!!error || undefined"
        v-bind="$attrs"
      >
        <slot />
      </select>

      <textarea
        v-else-if="as === 'textarea'"
        :id="id"
        :value="model as string"
        :rows="rows"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :class="controlClass"
        :aria-invalid="!!error || undefined"
        v-bind="$attrs"
        @input="onInput"
      />

      <input
        v-else
        :id="id"
        :value="model"
        :type="type"
        :placeholder="placeholder"
        :disabled="disabled"
        :required="required"
        :class="controlClass"
        :aria-invalid="!!error || undefined"
        v-bind="$attrs"
        @input="onInput"
      >

      <Icon
        v-if="as === 'select'"
        name="material-symbols:expand-more"
        class="pointer-events-none absolute right-2.5 top-1/2 size-5 -translate-y-1/2 text-on-surface-variant"
      />
    </div>

    <p v-if="error" class="font-label-sm text-label-sm text-error">{{ error }}</p>
    <p v-else-if="hint" class="font-label-sm text-label-sm text-on-surface-variant">{{ hint }}</p>
  </div>
</template>
