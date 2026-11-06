<script setup lang="ts">
import type { QuotationListItem } from '~/types/models'
import type { TableColumn, TableTab } from '~/types/table'

const api = useApi()
const toast = useToast()

const quotations = ref<QuotationListItem[]>([])
const loading = ref(true)
const search = ref('')
const status = ref('')
const page = ref(1)
const pageSize = ref(25)
const busyId = ref<number | null>(null)

const columns: TableColumn[] = [
  { key: 'number', label: 'Number', width: 'w-60' },
  { key: 'customer', label: 'Customer' },
  { key: 'date', label: 'Date', hideBelow: 'md' },
  { key: 'status', label: 'Status' },
  { key: 'grandTotal', label: 'Grand total', align: 'right', numeric: true }
]

// The quotations endpoint returns the full list, so filtering, tab counts and
// paging are all computed client-side.
const counts = computed(() => {
  const map: Record<string, number> = {}
  for (const q of quotations.value) map[q.status] = (map[q.status] ?? 0) + 1
  return map
})

const tabs = computed<TableTab[]>(() => [
  { value: '', label: 'All', count: quotations.value.length },
  { value: 'Draft', label: 'Draft', count: counts.value.Draft ?? 0 },
  { value: 'Sent', label: 'Sent', count: counts.value.Sent ?? 0 },
  { value: 'Accepted', label: 'Accepted', count: counts.value.Accepted ?? 0 }
])

const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  return quotations.value.filter((q) => {
    if (status.value && q.status !== status.value) return false
    if (!term) return true
    return (
      q.number.toLowerCase().includes(term) ||
      (q.customer ?? '').toLowerCase().includes(term)
    )
  })
})

const paged = computed(() =>
  filtered.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value)
)

watch([search, status], () => (page.value = 1))

async function load() {
  loading.value = true
  try {
    quotations.value = await api.getQuotations()
  } catch (error) {
    toast.error(error)
    quotations.value = []
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function markSent(q: QuotationListItem) {
  busyId.value = q.id
  try {
    await api.setQuotationStatus(q.id, 'Sent')
    toast.success(`${q.number} marked as sent`)
    await load()
  } catch (error) {
    toast.error(error)
  } finally {
    busyId.value = null
  }
}

const confirming = ref<QuotationListItem | null>(null)

async function convert() {
  const q = confirming.value
  if (!q) return

  busyId.value = q.id
  try {
    const result = await api.convertQuotation(q.id)
    toast.success(`${q.number} converted — invoice #${result.invoiceId}, dispatch #${result.dispatchId}`)
    confirming.value = null
    await load()
  } catch (error) {
    toast.error(error)
  } finally {
    busyId.value = null
  }
}
</script>

<template>
  <PageHeader
    title="Quotations"
    description="Draft, send and convert quotations into invoices and dispatch notes."
  >
    <template #actions>
      <AppButton icon="add" to="/quotations/new">New quotation</AppButton>
    </template>
  </PageHeader>

  <TableToolbar
    v-model:search="search"
    v-model:tab="status"
    :tabs="tabs"
    search-placeholder="Search number or customer..."
  />

  <DataTable
    :columns="columns"
    :rows="paged"
    :loading="loading"
    empty-title="No quotations yet"
    empty-description="Create a quotation to begin the sales flow."
    empty-icon="request-quote"
  >
    <template #cell-number="{ row }">
      <span class="font-label-lg text-label-lg text-on-surface">{{ row.number }}</span>
    </template>

    <template #cell-customer="{ value }">
      {{ value || '—' }}
    </template>

    <template #cell-date="{ value }">
      {{ formatDate(value) }}
    </template>

    <template #cell-status="{ value }">
      <StatusBadge :status="value" />
    </template>

    <template #cell-grandTotal="{ row }">
      <span class="font-label-lg text-label-lg">{{ formatNumber(row.totals.grandTotal) }}</span>
    </template>

    <template #actions="{ row }">
      <div class="flex items-center justify-end gap-space-xs">
        <AppButton
          v-if="row.status === 'Draft'"
          variant="ghost"
          size="sm"
          icon="send"
          :loading="busyId === row.id"
          @click="markSent(row)"
        >
          Mark sent
        </AppButton>

        <AppButton
          v-if="row.status !== 'Accepted'"
          variant="secondary"
          size="sm"
          icon="move-down"
          :loading="busyId === row.id"
          @click="confirming = row"
        >
          Convert
        </AppButton>

        <StatusBadge v-else status="Converted" icon="check" />
      </div>
    </template>

    <template #empty-action>
      <AppButton icon="add" to="/quotations/new">New quotation</AppButton>
    </template>
  </DataTable>

  <TablePagination v-model:page="page" v-model:page-size="pageSize" :total="filtered.length" />

  <AppModal
    :open="!!confirming"
    size="sm"
    title="Convert quotation"
    @close="confirming = null"
  >
    <p class="font-body-md text-body-md text-on-surface">
      This marks <strong>{{ confirming?.number }}</strong> as accepted and creates an invoice and a
      dispatch note from its lines.
    </p>
    <p class="mt-space-sm font-body-sm text-body-sm text-on-surface-variant">
      Stock is not deducted yet — that happens when the dispatch is issued. This cannot be undone.
    </p>

    <template #footer>
      <AppButton variant="secondary" @click="confirming = null">Cancel</AppButton>
      <AppButton :loading="busyId === confirming?.id" @click="convert">Convert</AppButton>
    </template>
  </AppModal>
</template>
