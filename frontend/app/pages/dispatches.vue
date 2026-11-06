<script setup lang="ts">
import type { TableColumn, TableTab } from '~/types/table'

const api = useApi()
const toast = useToast()

const dispatches = ref<any[]>([])
const loading = ref(true)
const search = ref('')
const status = ref('')
const page = ref(1)
const pageSize = ref(25)
const busyId = ref<number | null>(null)

const current = ref<any | null>(null)
const confirming = ref<any | null>(null)

const columns: TableColumn[] = [
  { key: 'number', label: 'Number', width: 'w-60' },
  { key: 'customer', label: 'Customer' },
  { key: 'date', label: 'Date', hideBelow: 'md' },
  { key: 'lineCount', label: 'Lines', align: 'right', numeric: true, hideBelow: 'lg' },
  { key: 'status', label: 'Status' }
]

const tabs = computed<TableTab[]>(() => [
  { value: '', label: 'All', count: dispatches.value.length },
  { value: 'pending', label: 'Pending', count: dispatches.value.filter((d) => !d.isIssued).length },
  { value: 'issued', label: 'Issued', count: dispatches.value.filter((d) => d.isIssued).length }
])

const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  return dispatches.value.filter((d) => {
    if (status.value === 'pending' && d.isIssued) return false
    if (status.value === 'issued' && !d.isIssued) return false
    if (!term) return true
    return (
      String(d.number).toLowerCase().includes(term) ||
      String(d.customer ?? '').toLowerCase().includes(term)
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
    dispatches.value = await api.getDispatches()
  } catch (error) {
    toast.error(error)
    dispatches.value = []
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function view(id: number) {
  try {
    current.value = await api.getDispatch(id)
  } catch (error) {
    toast.error(error)
  }
}

async function issue() {
  const dispatch = confirming.value
  if (!dispatch) return

  busyId.value = dispatch.id
  try {
    const result = await api.issueDispatch(dispatch.id)

    // Short stock is a warning, not a block — surface it rather than hide it.
    if (result.warnings?.length) {
      toast.warning(`${dispatch.number} issued with stock shortfalls: ${result.warnings.join('; ')}`, 10000)
    } else {
      toast.success(`${dispatch.number} issued — stock reduced`)
    }

    confirming.value = null
    await load()
  } catch (error) {
    toast.error(error)
  } finally {
    busyId.value = null
  }
}

function print() {
  window.print()
}

const dispatch = computed(() => current.value?.dispatch ?? null)
</script>

<template>
  <PageHeader
    title="Dispatch notes"
    description="Stock is deducted only when a dispatch is issued."
  />

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
    empty-title="No dispatch notes yet"
    empty-description="Convert an accepted quotation to generate one."
    empty-icon="local-shipping"
  >
    <template #cell-number="{ row }">
      <span class="font-label-lg text-label-lg text-on-surface">{{ row.number }}</span>
    </template>

    <template #cell-customer="{ value }">{{ value || '—' }}</template>

    <template #cell-date="{ value }">{{ formatDate(value) }}</template>

    <template #cell-status="{ row }">
      <StatusBadge
        :status="row.isIssued ? 'Issued' : 'Pending'"
        :tone="row.isIssued ? 'success' : 'warning'"
      />
    </template>

    <template #actions="{ row }">
      <div class="flex items-center justify-end gap-space-xs">
        <AppButton variant="ghost" size="sm" icon="print" @click="view(row.id)">View</AppButton>
        <AppButton
          v-if="!row.isIssued"
          variant="secondary"
          size="sm"
          icon="inventory"
          :loading="busyId === row.id"
          @click="confirming = row"
        >
          Issue
        </AppButton>
      </div>
    </template>
  </DataTable>

  <TablePagination v-model:page="page" v-model:page-size="pageSize" :total="filtered.length" />

  <!-- Issue confirmation -->
  <AppModal :open="!!confirming" size="sm" title="Issue dispatch" @close="confirming = null">
    <p class="font-body-md text-body-md text-on-surface">
      Issuing <strong>{{ confirming?.number }}</strong> deducts its quantities from stock.
    </p>
    <p class="mt-space-sm font-body-sm text-body-sm text-on-surface-variant">
      Service lines are skipped. If an item is short, the dispatch is still issued and a warning is
      reported — stock may go negative. This cannot be undone.
    </p>

    <template #footer>
      <AppButton variant="secondary" @click="confirming = null">Cancel</AppButton>
      <AppButton :loading="busyId === confirming?.id" @click="issue">Issue</AppButton>
    </template>
  </AppModal>

  <!-- Printable note -->
  <AppModal :open="!!current" size="xl" printable @close="current = null">
    <template #header>
      <div>
        <h2 class="font-headline-sm text-headline-sm text-on-surface">
          Dispatch {{ dispatch?.number }}
        </h2>
        <p class="font-body-sm text-body-sm text-on-surface-variant">
          {{ dispatch?.customer?.name || dispatch?.customerNameOverride }}
        </p>
      </div>
      <AppButton icon="print" class="ml-auto mr-space-sm" @click="print">Print</AppButton>
    </template>

    <PrintDocument
      v-if="dispatch"
      kind="Dispatch Note"
      :number="dispatch.number"
      :date="dispatch.date"
      :customer-name="dispatch.customer?.name || dispatch.customerNameOverride || 'Customer'"
      :billing-address="dispatch.customer?.billingAddress"
      :contact-person="dispatch.customer?.contactPerson"
      :phone="dispatch.customer?.phone"
      :work-ref="dispatch.workRef"
      :project-site="dispatch.projectSite"
      :po-no="dispatch.poNo"
      :po-date="dispatch.poDate"
      :job-ref="dispatch.jobRef"
      :watermark="dispatch.isIssued ? null : 'PENDING'"
    >
      <template #items>
        <table class="w-full border-collapse text-left">
          <thead>
            <tr class="bg-[#0f172a] font-label-md text-label-md uppercase tracking-wider text-white">
              <th class="w-10 px-2 py-2 font-semibold">#</th>
              <th class="px-2 py-2 font-semibold">Description</th>
              <th class="w-24 px-2 py-2 font-semibold">UoM</th>
              <th class="w-28 px-2 py-2 text-right font-semibold">Qty</th>
              <th class="w-40 px-2 py-2 font-semibold">Received</th>
            </tr>
          </thead>
          <tbody class="font-body-sm text-body-sm">
            <tr
              v-for="(item, index) in dispatch.items"
              :key="item.id"
              class="border-b border-[#e2e8f0]"
            >
              <td class="px-2 py-2 tabular text-[#64748b]">{{ index + 1 }}</td>
              <td class="px-2 py-2">{{ item.description }}</td>
              <td class="px-2 py-2">{{ item.unit }}</td>
              <td class="px-2 py-2 text-right tabular">{{ item.quantity }}</td>
              <!-- Left blank intentionally: signed off on paper at delivery. -->
              <td class="px-2 py-2" />
            </tr>
          </tbody>
        </table>
      </template>

      <template #totals>
        <p class="mt-space-md font-body-sm text-body-sm text-[#64748b]">
          Goods remain the property of E&amp;G Engineering Ltd. until paid in full. Please check
          quantities on receipt; discrepancies must be reported within 48 hours.
        </p>
      </template>
    </PrintDocument>
  </AppModal>
</template>
