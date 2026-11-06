<script setup lang="ts">
import type { Customer, DocHeader, Product, QuotationItem } from '~/types/models'

const api = useApi()
const toast = useToast()

const customers = ref<Customer[]>([])
const products = ref<Product[]>([])
const saving = ref(false)
const formError = ref('')

const model = reactive<DocHeader>({
  customerId: null,
  customerNameOverride: null,
  workRef: null,
  projectSite: null,
  poNo: null,
  poDate: null,
  jobRef: null,
  validUntil: null,
  documentDiscountPercent: 0,
  documentDiscountAmount: 0,
  vatPercent: 0,
  serviceCharge: 0,
  weightCharge: 0,
  deliveryCharge: 0,
  items: []
})

/** Sites belonging to the selected customer, for the project-site picker. */
const selectedCustomer = computed(() =>
  customers.value.find((c) => c.id === model.customerId) ?? null
)

// Live preview. The server recomputes on save; this mirrors its rules.
const totals = computed(() => computeTotals(model.items, model))

function addLine() {
  model.items.push({
    productId: null,
    description: '',
    unit: 'Pcs',
    quantity: 1,
    unitPrice: 0,
    lineDiscountPercent: 0,
    lineDiscountAmount: 0,
    remarks: null
  })
}

function removeLine(index: number) {
  model.items.splice(index, 1)
}

/** Copies catalogue defaults into the line when a product is picked. */
function onProduct(line: QuotationItem) {
  const product = products.value.find((p) => p.id === line.productId)
  if (!product) return
  line.description = product.name
  line.unit = product.unit
  line.unitPrice = product.sellingPrice
}

/** Warns when a line would ship more than is currently on hand. */
function shortfall(line: QuotationItem) {
  const product = products.value.find((p) => p.id === line.productId)
  if (!product || product.type === 'Service') return null
  const short = line.quantity - product.stockQuantity
  return short > 0 ? short : null
}

const shortLines = computed(() => model.items.filter((l) => shortfall(l) !== null).length)

async function save() {
  formError.value = ''

  if (model.items.length === 0) {
    formError.value = 'Add at least one line item.'
    return
  }
  if (!model.customerId && !model.customerNameOverride?.trim()) {
    formError.value = 'Select a customer or enter a one-off customer name.'
    return
  }
  if (model.items.some((l) => !l.description.trim())) {
    formError.value = 'Every line needs a description.'
    return
  }

  saving.value = true
  try {
    await api.createQuotation(model)
    toast.success('Quotation created')
    await navigateTo('/quotations')
  } catch (error: any) {
    formError.value = error?.data?.message || error?.message || 'Could not save the quotation.'
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  try {
    const [customerList, productPage] = await Promise.all([
      api.getCustomers(),
      api.getProducts({ pageSize: 200 })
    ])
    customers.value = customerList
    products.value = productPage.items
  } catch (error) {
    toast.error(error)
  }
  addLine()
})

const CARD = 'rounded-xl bg-surface-container-lowest p-space-lg elevation-1'
const CELL_INPUT =
  'h-9 w-full rounded-lg bg-surface-container-low px-2.5 font-body-sm text-body-sm text-on-surface ' +
  'transition-all focus:bg-surface-container-lowest focus:shadow-[0_0_0_2px_rgb(37_99_235_/_0.4)] focus:outline-none'
</script>

<template>
  <PageHeader title="New quotation" description="Build the document, then convert it when accepted.">
    <template #actions>
      <AppButton variant="secondary" to="/quotations">Cancel</AppButton>
      <AppButton :loading="saving" icon="save" @click="save">Save quotation</AppButton>
    </template>
  </PageHeader>

  <div class="grid grid-cols-1 gap-gutter xl:grid-cols-[1fr_20rem]">
    <div class="flex flex-col gap-gutter">
      <!-- Header -->
      <section :class="CARD">
        <h2 class="mb-space-md font-headline-sm text-headline-sm text-on-surface">Document details</h2>

        <div class="grid grid-cols-1 gap-space-md sm:grid-cols-2 lg:grid-cols-3">
          <FormField v-model="model.customerId" label="Customer" as="select">
            <option :value="null">One-off (free text)</option>
            <option v-for="c in customers" :key="c.id" :value="c.id">{{ c.name }}</option>
          </FormField>

          <FormField
            v-model="model.customerNameOverride"
            label="One-off customer name"
            :disabled="!!model.customerId"
            :hint="model.customerId ? 'Using the selected customer.' : undefined"
          />

          <FormField
            v-if="selectedCustomer?.sites.length"
            v-model="model.projectSite"
            label="Project site"
            as="select"
          >
            <option :value="null">Not specified</option>
            <option v-for="s in selectedCustomer.sites" :key="s.id ?? s.siteName" :value="s.siteName">
              {{ s.siteName }}
            </option>
          </FormField>
          <FormField v-else v-model="model.projectSite" label="Project site" />

          <FormField v-model="model.workRef" label="Work reference" />
          <FormField v-model="model.poNo" label="PO number" />
          <FormField v-model="model.jobRef" label="Job reference" />
          <FormField v-model="model.poDate" label="PO date" type="date" />
          <FormField v-model="model.validUntil" label="Valid until" type="date" />
        </div>
      </section>

      <!-- Lines -->
      <section :class="CARD">
        <div class="mb-space-md flex items-center justify-between gap-space-md">
          <div>
            <h2 class="font-headline-sm text-headline-sm text-on-surface">Line items</h2>
            <p class="font-label-sm text-label-sm text-on-surface-variant">
              Pick a catalogue item to auto-fill price and unit, or type a custom line.
            </p>
          </div>
          <AppButton variant="secondary" size="sm" icon="add" @click="addLine">Add line</AppButton>
        </div>

        <div v-if="model.items.length === 0">
          <EmptyState
            title="No lines yet"
            description="A quotation needs at least one line item."
            icon="list-alt"
          >
            <AppButton icon="add" @click="addLine">Add line</AppButton>
          </EmptyState>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full border-collapse text-left">
            <thead>
              <tr
                class="bg-surface-container-low font-label-md text-label-md uppercase tracking-wider text-on-surface-variant"
              >
                <th class="w-52 px-2 py-2.5 font-semibold">Product</th>
                <th class="px-2 py-2.5 font-semibold">Description</th>
                <th class="w-20 px-2 py-2.5 font-semibold">Unit</th>
                <th class="w-24 px-2 py-2.5 text-right font-semibold">Qty</th>
                <th class="w-32 px-2 py-2.5 text-right font-semibold">Price</th>
                <th class="w-20 px-2 py-2.5 text-right font-semibold">Disc %</th>
                <th class="w-32 px-2 py-2.5 text-right font-semibold">Net</th>
                <th class="w-10 px-2 py-2.5" />
              </tr>
            </thead>
            <tbody class="divide-y divide-surface-container">
              <tr v-for="(line, index) in model.items" :key="index">
                <td class="px-2 py-2">
                  <select v-model="line.productId" :class="CELL_INPUT" @change="onProduct(line)">
                    <option :value="null">Custom line</option>
                    <option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }}</option>
                  </select>
                </td>
                <td class="px-2 py-2">
                  <input v-model="line.description" :class="CELL_INPUT" placeholder="Description">
                  <p
                    v-if="shortfall(line)"
                    class="mt-1 flex items-center gap-1 font-label-sm text-label-sm text-amber-700"
                  >
                    <Icon name="material-symbols:warning-outline" class="size-3.5 shrink-0" />
                    Short by {{ shortfall(line) }} — stock is deducted at dispatch.
                  </p>
                </td>
                <td class="px-2 py-2">
                  <input v-model="line.unit" :class="CELL_INPUT">
                </td>
                <td class="px-2 py-2">
                  <input
                    v-model.number="line.quantity"
                    type="number"
                    min="0"
                    :class="[CELL_INPUT, 'text-right tabular']"
                  >
                </td>
                <td class="px-2 py-2">
                  <input
                    v-model.number="line.unitPrice"
                    type="number"
                    min="0"
                    step="0.01"
                    :class="[CELL_INPUT, 'text-right tabular']"
                  >
                </td>
                <td class="px-2 py-2">
                  <input
                    v-model.number="line.lineDiscountPercent"
                    type="number"
                    min="0"
                    max="100"
                    :class="[CELL_INPUT, 'text-right tabular']"
                  >
                </td>
                <td class="px-2 py-2 text-right font-label-lg text-label-lg tabular text-on-surface">
                  {{ formatNumber(lineNet(line)) }}
                </td>
                <td class="px-2 py-2">
                  <AppButton
                    variant="ghost"
                    size="icon"
                    icon="delete"
                    title="Remove line"
                    @click="removeLine(index)"
                  />
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Charges -->
      <section :class="CARD">
        <h2 class="mb-space-md font-headline-sm text-headline-sm text-on-surface">
          Discounts &amp; charges
        </h2>
        <div class="grid grid-cols-2 gap-space-md lg:grid-cols-3">
          <FormField
            v-model="model.documentDiscountPercent"
            label="Document discount %"
            type="number"
            numeric
            step="0.01"
          />
          <FormField
            v-model="model.documentDiscountAmount"
            label="Document discount amount"
            type="number"
            numeric
            step="0.01"
          />
          <FormField v-model="model.vatPercent" label="VAT %" type="number" numeric step="0.01" />
          <FormField
            v-model="model.serviceCharge"
            label="Service charge"
            type="number"
            numeric
            step="0.01"
          />
          <FormField
            v-model="model.weightCharge"
            label="Weight charge"
            type="number"
            numeric
            step="0.01"
          />
          <FormField
            v-model="model.deliveryCharge"
            label="Delivery charge"
            type="number"
            numeric
            step="0.01"
          />
        </div>
        <p class="mt-space-sm font-label-sm text-label-sm text-on-surface-variant">
          Charges are added to the discounted subtotal before VAT is applied.
        </p>
      </section>
    </div>

    <!-- Live totals -->
    <aside class="xl:sticky xl:top-[calc(4rem+var(--spacing-space-xl))] xl:self-start">
      <div :class="[CARD, 'flex flex-col gap-space-sm']">
        <h2 class="font-headline-sm text-headline-sm text-on-surface">Totals</h2>

        <dl class="flex flex-col gap-space-xs font-body-sm text-body-sm">
          <div class="flex justify-between gap-space-md">
            <dt class="text-on-surface-variant">Subtotal</dt>
            <dd class="tabular text-on-surface">{{ formatNumber(totals.subTotal) }}</dd>
          </div>
          <div class="flex justify-between gap-space-md">
            <dt class="text-on-surface-variant">Discount</dt>
            <dd class="tabular text-on-surface">-{{ formatNumber(totals.discount) }}</dd>
          </div>
          <div class="flex justify-between gap-space-md">
            <dt class="text-on-surface-variant">Charges</dt>
            <dd class="tabular text-on-surface">{{ formatNumber(totals.charges) }}</dd>
          </div>
          <div class="flex justify-between gap-space-md">
            <dt class="text-on-surface-variant">VAT ({{ model.vatPercent || 0 }}%)</dt>
            <dd class="tabular text-on-surface">{{ formatNumber(totals.vat) }}</dd>
          </div>
        </dl>

        <div
          class="mt-space-xs flex items-baseline justify-between gap-space-md border-t border-surface-container pt-space-sm"
        >
          <span class="font-label-lg text-label-lg text-on-surface-variant">Grand total</span>
          <span class="font-metric-val text-metric-val tabular text-on-surface">
            {{ formatNumber(totals.grandTotal) }}
          </span>
        </div>

        <p class="font-label-sm text-label-sm text-on-surface-variant">
          Preview only — the server recalculates on save.
        </p>

        <div
          v-if="shortLines > 0"
          class="mt-space-sm flex items-start gap-1.5 rounded-xl bg-amber-50 px-space-sm py-2 font-body-sm text-body-sm text-amber-700"
        >
          <Icon name="material-symbols:warning-outline" class="mt-0.5 size-4 shrink-0" />
          <span>{{ shortLines }} line{{ shortLines === 1 ? '' : 's' }} exceed current stock.</span>
        </div>

        <p
          v-if="formError"
          class="mt-space-sm flex items-start gap-1.5 rounded-xl bg-error-container px-space-sm py-2 font-body-sm text-body-sm text-on-error-container"
          role="alert"
        >
          <Icon name="material-symbols:error-outline" class="mt-0.5 size-4 shrink-0" />
          {{ formError }}
        </p>

        <AppButton class="mt-space-sm w-full" :loading="saving" icon="save" @click="save">
          Save quotation
        </AppButton>
      </div>
    </aside>
  </div>
</template>
