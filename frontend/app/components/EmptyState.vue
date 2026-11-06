<script setup lang="ts">
/** Placeholder shown when a list or table has no rows. */
defineProps<{
  title: string
  description?: string
  icon?: string
}>()
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-space-sm px-space-lg py-16 text-center">
    <span
      class="flex size-12 items-center justify-center rounded-full bg-surface-container text-on-surface-variant"
    >
      <Icon :name="`material-symbols:${icon ?? 'inbox'}`" class="size-6" />
    </span>
    <h3 class="font-headline-sm text-headline-sm text-on-surface">{{ title }}</h3>
    <p v-if="description" class="max-w-sm font-body-sm text-body-sm text-on-surface-variant">
      {{ description }}
    </p>
    <div v-if="$slots.default" class="mt-space-sm">
      <slot />
    </div>
  </div>
</template>
