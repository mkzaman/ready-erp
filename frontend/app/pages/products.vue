<script setup lang="ts">
import type { Category, Location, Product, ProductUpsert, StockState, Warehouse } from '~/types/models'
import type { TableColumn, TableTab } from '~/types/table'

const api = useApi()
const toast = useToast()
const route = useRoute()
const router = useRouter()

// ---------------------------------------------------------------------------
// Reference data
// ---------------------------------------------------------------------------
const categories = ref<Category[]>([])
const warehouses = ref<Warehouse[]>([])

// ---------------------------------------------------------------------------
// List state. Filtering and paging are server-side.
// ---------------------------------------------------------------------------
const rows = ref<Product[]>([])
const total = ref(0)
const loading = ref(true)

const search = ref((route.query.q as string) ?? '')
const status = ref<StockState | ''>((route.query.status as StockState) ?? '')
const categoryId = ref<number | null>(null)
const warehouseId = ref<number | null>(null)
const page = ref(1)
const pageSize = ref(25)
const selected = ref<number[]>([])

const stats = ref<{ skus: number; low: number; out: number; value: number; dispatched: number } | null>(null)

const tabs: TableTab[] = [
  { value: '', label: 'All' },
  { value: 'InStock', label: 'In stock' },
  { value: 'LowStock', label: 'Low' },
  { value: 'OutOfStock', label: 'Out' },
  { value: 'NotTracked', label: 'Services' }
]

const columns: TableColumn[] = [
  { key: 'name', label: 'Product', width: 'w-[26rem]' },
  { key: 'categoryName', label: 'Category', hideBelow: 'lg' },
  { key: 'locationCode', label: 'Location', hideBelow: 'xl' },
  { key: 'stockQuantity', label: 'Stock', width: 'w-44' },
  { key: 'sellingPrice', label: 'Selling', align: 'right', numeric: true, hideBelow: 'md' },
  { key: 'stockState', label: 'Status' }
]

/** Compact money, so a 9-million inventory value does not blow up the tile. */
function compactMoney(value: number) {
  return new Intl.NumberFormat('en-US', { notation: 'compact', maximumFractionDigits: 1 }).format(value)
}

async function loadStats() {
  try {
    const d = await api.getDashboard()
    stats.value = {
      skus: d.productCount,
      low: d.lowStockCount,
      out: d.outOfStockCount,
      value: d.totalStockValue,
      dispatched: d.unitsDispatchedToday
    }
  } catch (error) {
    toast.error(error)
  }
}

async function load() {
  loading.value = true
  try {
    const result = await api.getProducts({
      search: search.value,
      status: status.value,
      categoryId: categoryId.value,
      warehouseId: warehouseId.value,
      page: page.value,
      pageSize: pageSize.value
    })
    rows.value = result.items
    total.value = result.total
    selected.value = []
  } catch (error) {
    toast.error(error)
    rows.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

// Debounce typing so each keystroke does not hit the API.
let searchTimer: ReturnType<typeof setTimeout> | undefined
watch(search, (value) => {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    page.value = 1
    router.replace({ query: value ? { q: value } : {} })
    load()
  }, 300)
})

// Any filter change invalidates the current page.
watch([status, categoryId, warehouseId], () => {
  page.value = 1
  load()
})

watch([page, pageSize], load)

// The topbar search pushes ?q=, which may land while this page is already open.
watch(
  () => route.query.q,
  (q) => {
    const next = (q as string) ?? ''
    if (next !== search.value) search.value = next
  }
)

onMounted(async () => {
  await Promise.all([
    load(),
    loadStats(),
    api.getCategories().then((c) => (categories.value = c)).catch(() => {}),
    api.getWarehouses().then((w) => (warehouses.value = w)).catch(() => {})
  ])
})

// ---------------------------------------------------------------------------
// Editor
// ---------------------------------------------------------------------------
const EMPTY: ProductUpsert = {
  partNumber: '',
  name: '',
  unit: 'Pcs',
  type: 'Product',
  buyingPrice: 0,
  sellingPrice: 0,
  stockQuantity: 0,
  lowStockThreshold: null,
  categoryId: null,
  warehouseId: null,
  locationId: null
}

const open = ref(false)
const saving = ref(false)
const editingId = ref<number | null>(null)
const form = ref<ProductUpsert>({ ...EMPTY })
const formError = ref('')

/** Locations for the warehouse chosen in the form, for the dependent select. */
const formLocations = ref<Location[]>([])
const imagePreview = ref<string | null>(null)
const pendingFile = ref<File | null>(null)

const isService = computed(() => form.value.type === 'Service')

watch(
  () => form.value.warehouseId,
  async (id, previous) => {
    // Clear a bin that belongs to the previously selected warehouse.
    if (previous !== undefined && id !== previous) form.value.locationId = null
    formLocations.value = id ? await api.getWarehouseLocations(id).catch(() => []) : []
  }
)

function openCreate() {
  editingId.value = null
  form.value = { ...EMPTY }
  formLocations.value = []
  imagePreview.value = null
  pendingFile.value = null
  formError.value = ''
  open.value = true
}

function openEdit(product: Product) {
  editingId.value = product.id
  form.value = {
    partNumber: product.partNumber,
    name: product.name,
    unit: product.unit,
    type: product.type,
    buyingPrice: product.buyingPrice,
    sellingPrice: product.sellingPrice,
    stockQuantity: product.stockQuantity,
    lowStockThreshold: product.lowStockThreshold,
    categoryId: product.categoryId,
    warehouseId: product.warehouseId,
    locationId: product.locationId
  }
  imagePreview.value = product.imagePath
  pendingFile.value = null
  formError.value = ''
  open.value = true
}

function onFileChange(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return
  pendingFile.value = file
  imagePreview.value = URL.createObjectURL(file)
}

async function removeImage() {
  pendingFile.value = null
  imagePreview.value = null
  if (editingId.value) {
    await api.deleteProductImage(editingId.value).catch((error) => toast.error(error))
  }
}

async function save() {
  formError.value = ''

  if (!form.value.partNumber.trim() || !form.value.name.trim()) {
    formError.value = 'Part number and name are required.'
    return
  }

  saving.value = true
  try {
    // Services are not stock-tracked; do not persist a misleading quantity.
    const payload: ProductUpsert = {
      ...form.value,
      stockQuantity: isService.value ? 0 : form.value.stockQuantity,
      lowStockThreshold: isService.value ? null : form.value.lowStockThreshold
    }

    const saved = editingId.value
      ? await api.updateProduct(editingId.value, payload)
      : await api.createProduct(payload)

    // A new product has no id until it exists, so the image is uploaded after.
    if (pendingFile.value) {
      await api.uploadProductImage(saved.id, pendingFile.value)
    }

    toast.success(editingId.value ? `${saved.name} updated` : `${saved.name} created`)
    open.value = false
    await Promise.all([load(), loadStats()])
  } catch (error: any) {
    formError.value = error?.data?.message || error?.message || 'Could not save the product.'
  } finally {
    saving.value = false
  }
}

async function deactivate(product: Product) {
  try {
    await api.deactivateProduct(product.id)
    toast.success(`${product.name} archived`)
    await Promise.all([load(), loadStats()])
  } catch (error) {
    toast.error(error)
  }
}

async function bulkDeactivate() {
  const ids = [...selected.value]
  try {
    await Promise.all(ids.map((id) => api.deactivateProduct(id)))
    toast.success(`${ids.length} product${ids.length === 1 ? '' : 's'} archived`)
    await Promise.all([load(), loadStats()])
  } catch (error) {
    toast.error(error)
  }
}

const STATUS_LABEL: Record<StockState, string> = {
  InStock: 'In stock',
  LowStock: 'Low stock',
  OutOfStock: 'Out of stock',
  NotTracked: 'Service'
}
</script>

<template>
  <PageHeader
    title="Products &amp; Inventory"
    description="Catalogue, stock levels and warehouse slotting."
    badge="Live catalog"
  >
    <template #actions>
      <AppButton icon="add" @click="openCreate">New product</AppButton>
    </template>
  </PageHeader>

  <div class="mb-space-xl grid grid-cols-1 gap-gutter sm:grid-cols-2 xl:grid-cols-4">
    <KpiCard
      label="Total SKUs"
      :value="stats?.skus ?? 0"
      :loading="!stats"
      icon="inventory-2"
      caption="Active catalogue items"
    />
    <KpiCard
      label="Low stock"
      :value="stats?.low ?? 0"
      :loading="!stats"
      icon="warning"
      :tone="stats && stats.low > 0 ? 'warning' : 'default'"
      caption="At or below reorder point"
    />
    <KpiCard
      label="Out of stock"
      :value="stats?.out ?? 0"
      :loading="!stats"
      icon="production-quantity-limits"
      :tone="stats && stats.out > 0 ? 'danger' : 'default'"
      caption="Needs immediate restock"
    />
    <KpiCard
      label="Stock value"
      :value="stats ? compactMoney(stats.value) : '0'"
      :loading="!stats"
      icon="payments"
      caption="Valued at cost (BDT)"
    />
  </div>

  <TableToolbar
    v-model:search="search"
    v-model:tab="status"
    :tabs="tabs"
    :selected-count="selected.length"
    search-placeholder="Search part number, name or category..."
  >
    <template #filters>
      <select
        v-model="categoryId"
        aria-label="Filter by category"
        class="h-10 cursor-pointer appearance-none rounded-xl bg-surface-container-low pl-3 pr-8 font-label-md text-label-md text-on-surface transition-colors hover:bg-surface-container focus:outline-none"
      >
        <option :value="null">All categories</option>
        <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
      </select>

      <select
        v-model="warehouseId"
        aria-label="Filter by warehouse"
        class="h-10 cursor-pointer appearance-none rounded-xl bg-surface-container-low pl-3 pr-8 font-label-md text-label-md text-on-surface transition-colors hover:bg-surface-container focus:outline-none"
      >
        <option :value="null">All warehouses</option>
        <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
      </select>
    </template>

    <template #bulk>
      <AppButton variant="destructive" size="sm" icon="archive" @click="bulkDeactivate">
        Archive
      </AppButton>
    </template>
  </TableToolbar>

  <DataTable
    v-model:selected="selected"
    :columns="columns"
    :rows="rows"
    :loading="loading"
    selectable
    empty-title="No products match these filters"
    empty-description="Try clearing the search or status filter, or add a new product."
    empty-icon="inventory-2"
  >
    <template #cell-name="{ row }">
      <div class="flex items-center gap-space-sm">
        <img
          v-if="row.imagePath"
          :src="row.imagePath"
          :alt="row.name"
          class="size-10 shrink-0 rounded-lg object-cover"
        >
        <span
          v-else
          class="flex size-10 shrink-0 items-center justify-center rounded-lg bg-surface-container text-on-surface-variant"
        >
          <Icon
            :name="row.type === 'Service' ? 'material-symbols:handyman' : 'material-symbols:inventory-2'"
            class="size-5"
          />
        </span>
        <div class="min-w-0">
          <p class="truncate font-label-lg text-label-lg text-on-surface">{{ row.name }}</p>
          <p class="truncate font-label-sm text-label-sm text-on-surface-variant">
            {{ row.partNumber }}
          </p>
        </div>
      </div>
    </template>

    <template #cell-categoryName="{ value }">
      <StatusBadge v-if="value" :status="value" plain tone="neutral" />
      <span v-else class="text-on-surface-variant">&mdash;</span>
    </template>

    <template #cell-locationCode="{ row }">
      <div v-if="row.warehouseName" class="leading-tight">
        <p class="font-label-md text-label-md text-on-surface">{{ row.locationCode ?? '—' }}</p>
        <p class="font-label-sm text-label-sm text-on-surface-variant">{{ row.warehouseName }}</p>
      </div>
      <span v-else class="text-on-surface-variant">&mdash;</span>
    </template>

    <template #cell-stockQuantity="{ row }">
      <span v-if="row.type === 'Service'" class="font-label-md text-label-md text-on-surface-variant">
        Not tracked
      </span>
      <StockProgressBar
        v-else
        :value="row.stockQuantity"
        :threshold="row.lowStockThreshold"
      />
    </template>

    <template #cell-sellingPrice="{ value }">
      {{ formatNumber(value) }}
    </template>

    <template #cell-stockState="{ value }">
      <StatusBadge :status="STATUS_LABEL[value as StockState]" />
    </template>

    <template #actions="{ row }">
      <div class="flex items-center justify-end gap-1">
        <AppButton variant="ghost" size="icon" icon="edit" title="Edit" @click="openEdit(row)" />
        <AppButton
          variant="ghost"
          size="icon"
          icon="archive"
          title="Archive"
          @click="deactivate(row)"
        />
      </div>
    </template>

    <template #empty-action>
      <AppButton icon="add" @click="openCreate">New product</AppButton>
    </template>
  </DataTable>

  <TablePagination v-model:page="page" v-model:page-size="pageSize" :total="total" />

  <!-- Editor -->
  <AppModal
    :open="open"
    variant="drawer"
    :title="editingId ? 'Edit product' : 'New product'"
    :description="editingId ? form.partNumber : 'Add an item to the catalogue.'"
    @close="open = false"
  >
    <div class="flex flex-col gap-space-lg">
      <!-- Image -->
      <div class="flex items-center gap-space-md">
        <img
          v-if="imagePreview"
          :src="imagePreview"
          alt=""
          class="size-20 shrink-0 rounded-xl object-cover"
        >
        <span
          v-else
          class="flex size-20 shrink-0 items-center justify-center rounded-xl bg-surface-container text-on-surface-variant"
        >
          <Icon name="material-symbols:image-outline" class="size-7" />
        </span>

        <div class="flex flex-col gap-space-xs">
          <div class="flex items-center gap-space-sm">
            <label
              class="inline-flex cursor-pointer items-center gap-space-xs rounded-xl bg-surface-container-lowest px-space-md py-2 font-label-lg text-label-lg text-on-surface elevation-1 transition-colors hover:bg-surface-container-low"
            >
              <Icon name="material-symbols:upload" class="size-[18px]" />
              {{ imagePreview ? 'Replace' : 'Upload' }}
              <input type="file" accept="image/*" class="hidden" @change="onFileChange">
            </label>
            <AppButton
              v-if="imagePreview"
              variant="ghost"
              size="sm"
              icon="delete"
              @click="removeImage"
            >
              Remove
            </AppButton>
          </div>
          <p class="font-label-sm text-label-sm text-on-surface-variant">
            JPG, PNG, WEBP or GIF. Max 5 MB.
          </p>
        </div>
      </div>

      <div class="grid grid-cols-1 gap-space-md sm:grid-cols-2">
        <FormField v-model="form.partNumber" label="Part number" required />
        <FormField v-model="form.name" label="Name" required />

        <FormField v-model="form.type" label="Type" as="select">
          <option value="Product">Product</option>
          <option value="Service">Service</option>
        </FormField>
        <FormField v-model="form.unit" label="Unit" />

        <FormField v-model="form.categoryId" label="Category" as="select">
          <option :value="null">No category</option>
          <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
        </FormField>

        <FormField
          v-model="form.buyingPrice"
          label="Buying price"
          type="number"
          numeric
          step="0.01"
        />
        <FormField
          v-model="form.sellingPrice"
          label="Selling price"
          type="number"
          numeric
          step="0.01"
        />

        <template v-if="!isService">
          <FormField
            v-model="form.stockQuantity"
            label="Stock quantity"
            type="number"
            numeric
          />
          <FormField
            v-model="form.lowStockThreshold"
            label="Reorder point"
            type="number"
            numeric
            hint="Leave blank for no low-stock alert."
          />
        </template>
      </div>

      <div v-if="!isService" class="grid grid-cols-1 gap-space-md sm:grid-cols-2">
        <FormField v-model="form.warehouseId" label="Warehouse" as="select">
          <option :value="null">Unassigned</option>
          <option v-for="w in warehouses" :key="w.id" :value="w.id">{{ w.name }}</option>
        </FormField>

        <FormField
          v-model="form.locationId"
          label="Location"
          as="select"
          :disabled="!form.warehouseId"
          :hint="form.warehouseId ? undefined : 'Pick a warehouse first.'"
        >
          <option :value="null">Unassigned</option>
          <option v-for="l in formLocations" :key="l.id" :value="l.id">
            {{ l.code }}<template v-if="l.description"> — {{ l.description }}</template>
          </option>
        </FormField>
      </div>

      <p
        v-if="formError"
        class="flex items-center gap-1.5 rounded-xl bg-error-container px-space-sm py-2 font-body-sm text-body-sm text-on-error-container"
        role="alert"
      >
        <Icon name="material-symbols:error-outline" class="size-4 shrink-0" />
        {{ formError }}
      </p>
    </div>

    <template #footer>
      <AppButton variant="secondary" @click="open = false">Cancel</AppButton>
      <AppButton :loading="saving" @click="save">
        {{ editingId ? 'Save changes' : 'Create product' }}
      </AppButton>
    </template>
  </AppModal>
</template>
