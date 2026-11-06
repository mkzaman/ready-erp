<script setup lang="ts">
import type { TableColumn, TableTab } from '~/types/table'

const api = useApi()
const toast = useToast()

const invoices = ref<any[]>([])
const loading = ref(true)
const search = ref('')
const status = ref('')
const page = ref(1)
const pageSize = ref(25)

const current = ref<any | null>(null)
const loadingDoc = ref(false)

const columns: TableColumn[] = [
  { key: 'number', label: 'Number', width: 'w-60' },
  { key: 'customer', label: 'Customer' },
  { key: 'date', label: 'Date', hideBelow: 'md' },
  { key: 'status', label: 'Status' },
  { key: 'grandTotal', label: 'Grand total', align: 'right', numeric: true }
]

const tabs = computed<TableTab[]>(() => [
  { value: '', label: 'All', count: invoices.value.length },
  { value: 'active', label: 'Active', count: invoices.value.filter((i) => !i.isVoided).length },
  { value: 'voided', label: 'Voided', count: invoices.value.filter((i) => i.isVoided).length }
])

const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  return invoices.value.filter((i) => {
    if (status.value === 'active' && i.isVoided) return false
    if (status.value === 'voided' && !i.isVoided) return false
    if (!term) return true
    return (
      String(i.number).toLowerCase().includes(term) ||
      String(i.customer ?? '').toLowerCase().includes(term)
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
    invoices.value = await api.getInvoices()
  } catch (error) {
    toast.error(error)
    invoices.value = []
  } finally {
    loading.value = false
  }
}

onMounted(load)

async function view(id: number) {
  loadingDoc.value = true
  try {
    current.value = await api.getInvoice(id)
  } catch (error) {
    toast.error(error)
  } finally {
    loadingDoc.value = false
  }
}

function print() {
  window.print()
}

const invoice = computed(() => current.value?.invoice ?? null)
const totals = computed(() => current.value?.totals ?? null)
</script>

<template>
  <PageHeader
    title="Invoices"
    description="Generated when a quotation is converted. Print or void from here."
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
    empty-title="No invoices yet"
    empty-description="Convert an accepted quotation to generate one."
    empty-icon="receipt-long"
  >
    <template #cell-number="{ row }">
      <span class="font-label-lg text-label-lg text-on-surface">{{ row.number }}</span>
    </template>

    <template #cell-customer="{ value }">{{ value || '—' }}</template>

    <template #cell-date="{ value }">{{ formatDate(value) }}</template>

    <template #cell-status="{ row }">
      <StatusBadge :status="row.isVoided ? 'Voided' : 'Active'" :tone="row.isVoided ? 'danger' : 'success'" />
    </template>

    <template #cell-grandTotal="{ row }">
      <span class="font-label-lg text-label-lg" :class="row.isVoided ? 'line-through opacity-60' : ''">
        {{ formatNumber(row.totals.grandTotal) }}
      </span>
    </template>

    <template #actions="{ row }">
      <AppButton variant="secondary" size="sm" icon="print" @click="view(row.id)">
        View
      </AppButton>
    </template>
  </DataTable>

  <TablePagination v-model:page="page" v-model:page-size="pageSize" :total="filtered.length" />

  <AppModal :open="!!current" size="xl" printable @close="current = null">
    <template #header>
      <div>
        <h2 class="font-headline-sm text-headline-sm text-on-surface">
          Invoice {{ invoice?.number }}
        </h2>
        <p class="font-body-sm text-body-sm text-on-surface-variant">
          {{ invoice?.customer?.name || invoice?.customerNameOverride }}
        </p>
      </div>
      <AppButton icon="print" class="ml-auto mr-space-sm" @click="print">Print</AppButton>
    </template>

    <PrintDocument
      v-if="invoice"
      kind="Invoice"
      :number="invoice.number"
      :date="invoice.date"
      :customer-name="invoice.customer?.name || invoice.customerNameOverride || 'Customer'"
      :billing-address="invoice.customer?.billingAddress"
      :contact-person="invoice.customer?.contactPerson"
      :phone="invoice.customer?.phone"
      :vat-bin="invoice.customer?.vatBin"
      :work-ref="invoice.workRef"
      :project-site="invoice.projectSite"
      :po-no="invoice.poNo"
      :po-date="invoice.poDate"
      :job-ref="invoice.jobRef"
      :watermark="invoice.isVoided ? 'VOIDED' : null"
    >
      <template #items>
        <table class="w-full border-collapse text-left">
          <thead>
            <tr class="bg-[#0f172a] font-label-md text-label-md uppercase tracking-wider text-white">
              <th class="w-10 px-2 py-2 font-semibold">#</th>
              <th class="px-2 py-2 font-semibold">Description</th>
              <th class="w-20 px-2 py-2 font-semibold">UoM</th>
              <th class="w-20 px-2 py-2 text-right font-semibold">Qty</th>
              <th class="w-32 px-2 py-2 text-right font-semibold">Unit price</th>
              <th class="w-32 px-2 py-2 text-right font-semibold">Amount</th>
            </tr>
          </thead>
          <tbody class="font-body-sm text-body-sm">
            <tr
              v-for="(item, index) in invoice.items"
              :key="item.id"
              class="border-b border-[#e2e8f0]"
            >
              <td class="px-2 py-2 tabular text-[#64748b]">{{ index + 1 }}</td>
              <td class="px-2 py-2">
                {{ item.description }}
                <span v-if="item.remarks" class="block text-[#64748b]">{{ item.remarks }}</span>
              </td>
              <td class="px-2 py-2">{{ item.unit }}</td>
              <td class="px-2 py-2 text-right tabular">{{ item.quantity }}</td>
              <td class="px-2 py-2 text-right tabular">{{ formatNumber(item.unitPrice) }}</td>
              <td class="px-2 py-2 text-right tabular">
                {{ formatNumber(lineNet(item)) }}
              </td>
            </tr>
          </tbody>
        </table>
      </template>

      <template #totals>
        <div class="mt-space-md flex justify-end break-inside-avoid">
          <dl class="w-80 font-body-sm text-body-sm">
            <div class="flex justify-between py-1">
              <dt class="text-[#64748b]">Sub-total</dt>
              <dd class="tabular">{{ formatNumber(totals.subTotal) }}</dd>
            </div>
            <div v-if="totals.discount" class="flex justify-between py-1">
              <dt class="text-[#64748b]">Discount</dt>
              <dd class="tabular">-{{ formatNumber(totals.discount) }}</dd>
            </div>
            <div v-if="totals.charges" class="flex justify-between py-1">
              <dt class="text-[#64748b]">Charges</dt>
              <dd class="tabular">{{ formatNumber(totals.charges) }}</dd>
            </div>
            <div v-if="invoice.vatPercent" class="flex justify-between py-1">
              <dt class="text-[#64748b]">VAT ({{ invoice.vatPercent }}%)</dt>
              <dd class="tabular">{{ formatNumber(totals.vat) }}</dd>
            </div>
            <div
              class="mt-1 flex justify-between border-t-2 border-[#2563eb] pt-2 font-headline-sm text-headline-sm"
            >
              <dt>Grand total</dt>
              <dd class="tabular">{{ formatNumber(totals.grandTotal) }}</dd>
            </div>
            <p class="mt-1 text-right font-body-sm text-body-sm italic text-[#64748b]">
              {{ totals.grandTotalInWords }}
            </p>
          </dl>
        </div>
      </template>
    </PrintDocument>
  </AppModal>
</template>
