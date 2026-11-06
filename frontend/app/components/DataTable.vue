<script setup lang="ts" generic="T extends Record<string, any>">
import type { TableColumn } from '~/types/table'

/**
 * Shared data table. Replaces the hand-written thead/tbody markup duplicated
 * across six pages.
 *
 * Per-cell overrides use dynamic slots: `#cell-partNumber="{ row, value }"`.
 * A `#actions="{ row }"` slot renders a right-aligned trailing column.
 */
const props = withDefaults(
  defineProps<{
    columns: TableColumn[]
    rows: T[]
    /** Property used as the Vue key and as the selection identity. */
    rowKey?: keyof T & string
    loading?: boolean
    selectable?: boolean
    emptyTitle?: string
    emptyDescription?: string
    emptyIcon?: string
  }>(),
  {
    rowKey: 'id' as keyof T & string,
    emptyTitle: 'Nothing to show',
    emptyIcon: 'inbox'
  }
)

/** Selected row keys. */
const selected = defineModel<Array<T[keyof T]>>('selected', { default: () => [] })

const ALIGN = {
  left: 'text-left',
  right: 'text-right',
  center: 'text-center'
} as const

const HIDE_BELOW = {
  sm: 'hidden sm:table-cell',
  md: 'hidden md:table-cell',
  lg: 'hidden lg:table-cell',
  xl: 'hidden xl:table-cell'
} as const

function cellClass(col: TableColumn) {
  return [
    'px-space-md py-3.5',
    ALIGN[col.align ?? 'left'],
    col.width ?? '',
    col.numeric ? 'tabular' : '',
    col.hideBelow ? HIDE_BELOW[col.hideBelow] : ''
  ]
}

const allSelected = computed(
  () => props.rows.length > 0 && selected.value.length === props.rows.length
)
const someSelected = computed(
  () => selected.value.length > 0 && selected.value.length < props.rows.length
)

function toggleAll() {
  selected.value = allSelected.value ? [] : props.rows.map((r) => r[props.rowKey])
}

function toggleRow(row: T) {
  const id = row[props.rowKey]
  selected.value = selected.value.includes(id)
    ? selected.value.filter((v) => v !== id)
    : [...selected.value, id]
}

const colSpan = computed(
  () => props.columns.length + (props.selectable ? 1 : 0) + 1
)
</script>

<template>
  <div class="overflow-x-auto bg-surface-container-lowest elevation-1">
    <table class="w-full border-collapse text-left">
      <thead>
        <tr
          class="bg-surface-container-low font-label-md text-label-md uppercase tracking-wider text-on-surface-variant"
        >
          <th v-if="selectable" scope="col" class="w-10 px-space-md py-3.5">
            <input
              type="checkbox"
              class="size-4 cursor-pointer rounded accent-primary-container"
              :checked="allSelected"
              :indeterminate="someSelected"
              aria-label="Select all rows"
              @change="toggleAll"
            >
          </th>

          <th
            v-for="col in columns"
            :key="col.key"
            scope="col"
            class="font-semibold"
            :class="cellClass(col)"
          >
            {{ col.label }}
          </th>

          <th scope="col" class="w-px px-space-md py-3.5 text-right font-semibold">
            <span class="sr-only">Actions</span>
          </th>
        </tr>
      </thead>

      <tbody
        class="divide-y divide-surface-container font-body-md text-body-md text-on-surface"
      >
        <tr v-if="loading">
          <td :colspan="colSpan" class="px-space-md py-space-lg">
            <LoadingSkeleton :rows="5" :columns="Math.min(columns.length, 6)" />
          </td>
        </tr>

        <tr v-else-if="rows.length === 0">
          <td :colspan="colSpan">
            <EmptyState
              :title="emptyTitle"
              :description="emptyDescription"
              :icon="emptyIcon"
            >
              <slot name="empty-action" />
            </EmptyState>
          </td>
        </tr>

        <tr
          v-for="row in rows"
          v-else
          :key="String(row[rowKey])"
          class="group transition-colors hover:bg-surface-container-low/60"
          :class="selected.includes(row[rowKey]) ? 'bg-surface-container-low' : ''"
        >
          <td v-if="selectable" class="px-space-md py-3.5">
            <input
              type="checkbox"
              class="size-4 cursor-pointer rounded accent-primary-container"
              :checked="selected.includes(row[rowKey])"
              :aria-label="`Select row ${row[rowKey]}`"
              @change="toggleRow(row)"
            >
          </td>

          <td v-for="col in columns" :key="col.key" :class="cellClass(col)">
            <slot :name="`cell-${col.key}`" :row="row" :value="row[col.key]">
              {{ row[col.key] }}
            </slot>
          </td>

          <td class="px-space-md py-3.5 text-right whitespace-nowrap">
            <slot name="actions" :row="row" />
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>
