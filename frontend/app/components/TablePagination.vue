<script setup lang="ts">
/**
 * Footer bar for <DataTable>. Visually fused to the table via `rounded-b-xl`.
 *
 * Emits via v-model on `page` and `pageSize`; the parent decides whether that
 * triggers a refetch (server-side) or a recompute (client-side).
 */
const props = withDefaults(
  defineProps<{
    total: number
    pageSizes?: number[]
  }>(),
  {
    pageSizes: () => [10, 25, 50, 100]
  }
)

const page = defineModel<number>('page', { default: 1 })
const pageSize = defineModel<number>('pageSize', { default: 25 })

const pageCount = computed(() => Math.max(1, Math.ceil(props.total / pageSize.value)))

const from = computed(() => (props.total === 0 ? 0 : (page.value - 1) * pageSize.value + 1))
const to = computed(() => Math.min(page.value * pageSize.value, props.total))

/** Windowed page list: 1 … n-1 n n+1 … last */
const pages = computed<Array<number | '...'>>(() => {
  const last = pageCount.value
  const current = page.value
  if (last <= 7) return Array.from({ length: last }, (_, i) => i + 1)

  const out: Array<number | '...'> = [1]
  const start = Math.max(2, current - 1)
  const end = Math.min(last - 1, current + 1)

  if (start > 2) out.push('...')
  for (let i = start; i <= end; i++) out.push(i)
  if (end < last - 1) out.push('...')
  out.push(last)
  return out
})

function go(target: number) {
  page.value = Math.max(1, Math.min(pageCount.value, target))
}

// Clamp back into range when the result set shrinks (e.g. after filtering).
watch(pageCount, (count) => {
  if (page.value > count) page.value = count
})

// Changing page size should return to the first page.
watch(pageSize, () => {
  page.value = 1
})
</script>

<template>
  <div
    class="flex flex-col items-center justify-between gap-space-md rounded-b-xl bg-surface-container-lowest p-space-md elevation-1 sm:flex-row"
  >
    <p class="font-label-md text-label-md text-on-surface-variant">
      Showing <span class="tabular font-semibold text-on-surface">{{ from }}-{{ to }}</span>
      of <span class="tabular font-semibold text-on-surface">{{ total }}</span>
    </p>

    <div class="flex items-center gap-space-md">
      <label class="flex items-center gap-space-xs font-label-md text-label-md text-on-surface-variant">
        Rows
        <select
          v-model.number="pageSize"
          class="h-8 cursor-pointer appearance-none rounded-lg bg-surface-container-low pl-2.5 pr-7 font-label-md text-label-md text-on-surface transition-colors hover:bg-surface-container focus:outline-none"
        >
          <option v-for="size in pageSizes" :key="size" :value="size">{{ size }}</option>
        </select>
      </label>

      <nav class="flex items-center gap-1" aria-label="Pagination">
        <button
          type="button"
          class="flex items-center gap-1 rounded-lg bg-surface-container-low px-2.5 py-1.5 font-label-md text-label-md text-on-surface-variant transition-colors hover:bg-surface-container hover:text-on-surface disabled:opacity-40"
          :disabled="page <= 1"
          @click="go(page - 1)"
        >
          <Icon name="material-symbols:chevron-left" class="size-4" />
          <span class="hidden sm:inline">Prev</span>
        </button>

        <template v-for="(p, i) in pages" :key="`${p}-${i}`">
          <span
            v-if="p === '...'"
            class="px-1 font-label-md text-label-md text-on-surface-variant"
          >…</span>
          <button
            v-else
            type="button"
            :aria-current="p === page ? 'page' : undefined"
            class="size-8 rounded-lg font-label-md text-label-md tabular transition-colors"
            :class="
              p === page
                ? 'bg-primary-container font-semibold text-on-primary'
                : 'bg-surface-container-low text-on-surface hover:bg-surface-container'
            "
            @click="go(p as number)"
          >
            {{ p }}
          </button>
        </template>

        <button
          type="button"
          class="flex items-center gap-1 rounded-lg bg-surface-container-low px-2.5 py-1.5 font-label-md text-label-md text-on-surface-variant transition-colors hover:bg-surface-container hover:text-on-surface disabled:opacity-40"
          :disabled="page >= pageCount"
          @click="go(page + 1)"
        >
          <span class="hidden sm:inline">Next</span>
          <Icon name="material-symbols:chevron-right" class="size-4" />
        </button>
      </nav>
    </div>
  </div>
</template>
