<script setup lang="ts">
import type { DashboardData, StockState } from '~/types/models'
import type { TableColumn } from '~/types/table'

const api = useApi()
const toast = useToast()

const data = ref<DashboardData | null>(null)
const loading = ref(true)

const columns: TableColumn[] = [
  { key: 'name', label: 'Product', width: 'w-[26rem]' },
  { key: 'categoryName', label: 'Category', hideBelow: 'lg' },
  { key: 'stockQuantity', label: 'Stock on hand', width: 'w-48' },
  { key: 'stockState', label: 'Status' }
]

function compactMoney(value: number) {
  return new Intl.NumberFormat('en-US', { notation: 'compact', maximumFractionDigits: 1 }).format(value)
}

const STATUS_LABEL: Record<StockState, string> = {
  InStock: 'In stock',
  LowStock: 'Low stock',
  OutOfStock: 'Out of stock',
  NotTracked: 'Service'
}

/** Week-on-week direction for the dispatch tile. */
const dispatchTrend = computed(() => {
  const series = data.value?.dispatchTrend ?? []
  if (series.length < 2) return { trend: 'flat' as const, delta: undefined }

  const today = series[series.length - 1] ?? 0
  const yesterday = series[series.length - 2] ?? 0
  const change = today - yesterday

  return {
    trend: change > 0 ? ('up' as const) : change < 0 ? ('down' as const) : ('flat' as const),
    delta: change === 0 ? 'No change' : `${change > 0 ? '+' : ''}${change} vs yesterday`
  }
})

const hasDispatchActivity = computed(() => (data.value?.dispatchTrend ?? []).some((n) => n > 0))

onMounted(async () => {
  try {
    data.value = await api.getDashboard()
  } catch (error) {
    toast.error(error)
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <PageHeader
    title="Operations overview"
    description="Stock health, documents in flight and items needing attention."
  >
    <template #actions>
      <AppButton variant="secondary" icon="inventory-2" to="/products">View inventory</AppButton>
      <AppButton icon="add" to="/quotations/new">New quotation</AppButton>
    </template>
  </PageHeader>

  <!-- Stock health -->
  <div class="mb-space-xl grid grid-cols-1 gap-gutter sm:grid-cols-2 xl:grid-cols-4">
    <KpiCard
      label="Stock value"
      :value="data ? compactMoney(data.totalStockValue) : '0'"
      :loading="loading"
      icon="payments"
      caption="Valued at cost (BDT)"
    />
    <KpiCard
      label="Low stock"
      :value="data?.lowStockCount ?? 0"
      :loading="loading"
      icon="warning"
      :tone="data && data.lowStockCount > 0 ? 'warning' : 'default'"
      caption="At or below reorder point"
    />
    <KpiCard
      label="Out of stock"
      :value="data?.outOfStockCount ?? 0"
      :loading="loading"
      icon="production-quantity-limits"
      :tone="data && data.outOfStockCount > 0 ? 'danger' : 'default'"
      caption="Needs immediate restock"
    />
    <KpiCard
      label="Dispatched today"
      :value="data?.unitsDispatchedToday ?? 0"
      :loading="loading"
      icon="local-shipping"
      :series="hasDispatchActivity ? data?.dispatchTrend : undefined"
      :delta="dispatchTrend.delta"
      :trend="dispatchTrend.trend"
      caption="Units issued from stock"
    />
  </div>

  <!-- Document counts -->
  <div class="mb-space-xl grid grid-cols-2 gap-gutter lg:grid-cols-4">
    <KpiCard
      label="Products"
      :value="data?.productCount ?? 0"
      :loading="loading"
      icon="inventory-2"
    />
    <KpiCard
      label="Customers"
      :value="data?.customerCount ?? 0"
      :loading="loading"
      icon="groups"
    />
    <KpiCard
      label="Quotations"
      :value="data?.quotationCount ?? 0"
      :loading="loading"
      icon="request-quote"
    />
    <KpiCard
      label="Invoices"
      :value="data?.invoiceCount ?? 0"
      :loading="loading"
      icon="receipt-long"
      caption="Excluding voided"
    />
  </div>

  <!-- Pending dispatches callout -->
  <div
    v-if="data && data.dispatchesPendingIssue > 0"
    class="mb-space-xl flex items-center gap-space-sm rounded-xl bg-surface-container-lowest p-space-md elevation-1"
  >
    <span class="flex size-10 shrink-0 items-center justify-center rounded-xl bg-surface-container text-primary">
      <Icon name="material-symbols:pending-actions" class="size-5" />
    </span>
    <div class="flex-1">
      <p class="font-headline-sm text-headline-sm text-on-surface">
        {{ data.dispatchesPendingIssue }}
        dispatch{{ data.dispatchesPendingIssue === 1 ? '' : 'es' }} awaiting issue
      </p>
      <p class="font-body-sm text-body-sm text-on-surface-variant">
        Stock is only deducted once a dispatch is issued.
      </p>
    </div>
    <AppButton variant="secondary" icon="local-shipping" to="/dispatches">Review</AppButton>
  </div>

  <!-- Reorder list -->
  <div class="mb-space-md flex items-center justify-between gap-space-md">
    <div>
      <h2 class="font-headline-md text-headline-md text-on-surface">Reorder list</h2>
      <p class="font-body-sm text-body-sm text-on-surface-variant">
        Items at or below their reorder point, worst first.
      </p>
    </div>
    <AppButton variant="secondary" size="sm" icon="arrow-forward" icon-trailing to="/products?status=LowStock">
      All products
    </AppButton>
  </div>

  <DataTable
    :columns="columns"
    :rows="data?.reorderList ?? []"
    :loading="loading"
    empty-title="Nothing needs reordering"
    empty-description="Every tracked item is above its reorder point."
    empty-icon="check-circle"
  >
    <template #cell-name="{ row }">
      <div class="min-w-0">
        <p class="truncate font-label-lg text-label-lg text-on-surface">{{ row.name }}</p>
        <p class="truncate font-label-sm text-label-sm text-on-surface-variant">{{ row.partNumber }}</p>
      </div>
    </template>

    <template #cell-categoryName="{ value }">
      <StatusBadge v-if="value" :status="value" plain tone="neutral" />
      <span v-else class="text-on-surface-variant">&mdash;</span>
    </template>

    <template #cell-stockQuantity="{ row }">
      <StockProgressBar :value="row.stockQuantity" :threshold="row.lowStockThreshold" />
    </template>

    <template #cell-stockState="{ value }">
      <StatusBadge :status="STATUS_LABEL[value as StockState]" />
    </template>

    <template #actions="{ row }">
      <AppButton
        variant="ghost"
        size="icon"
        icon="edit"
        title="Open in products"
        :to="`/products?q=${encodeURIComponent(row.partNumber)}`"
      />
    </template>
  </DataTable>
</template>
