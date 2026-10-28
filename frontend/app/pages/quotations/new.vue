<script setup lang="ts">
import type { Customer, DocHeader, Product, QuotationItem } from '~/types/models'

const api = useApi()
const customers = ref<Customer[]>([])
const products = ref<Product[]>([])

const model = reactive<DocHeader>({
  customerId: null, customerNameOverride: null, workRef: null, projectSite: null,
  poNo: null, poDate: null, jobRef: null, validUntil: null,
  documentDiscountPercent: 0, documentDiscountAmount: 0, vatPercent: 0,
  serviceCharge: 0, weightCharge: 0, deliveryCharge: 0, items: []
})

function addLine() {
  model.items.push({
    productId: null, description: '', unit: 'Pcs', quantity: 1,
    unitPrice: 0, lineDiscountPercent: 0, lineDiscountAmount: 0, remarks: null
  })
}
function removeLine(i: number) {
  model.items.splice(i, 1)
}
function onProduct(line: QuotationItem) {
  const p = products.value.find((x) => x.id === line.productId)
  if (p) {
    line.description = p.name
    line.unit = p.unit
    line.unitPrice = p.sellingPrice
  }
}

async function save() {
  await api.createQuotation(model)
  await navigateTo('/quotations')
}

onMounted(async () => {
  customers.value = await api.getCustomers()
  products.value = await api.getProducts()
})
</script>

<template>
  <h2 class="text-[#0f2b46] text-xl font-bold mb-4">New Quotation</h2>

  <div class="bg-white p-5 rounded-lg mb-4">
    <div class="grid grid-cols-3 gap-3">
      <label class="flex flex-col text-xs font-semibold text-slate-700">Customer
        <select v-model="model.customerId" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal">
          <option :value="null">-- One-off (free text) --</option>
          <option v-for="c in customers" :key="c.id" :value="c.id">{{ c.name }}</option>
        </select>
      </label>
      <label class="flex flex-col text-xs font-semibold text-slate-700">Or Customer Name
        <input v-model="model.customerNameOverride" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
      </label>
      <label class="flex flex-col text-xs font-semibold text-slate-700">Work Ref
        <input v-model="model.workRef" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
      </label>
      <label class="flex flex-col text-xs font-semibold text-slate-700">Project Site
        <input v-model="model.projectSite" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
      </label>
      <label class="flex flex-col text-xs font-semibold text-slate-700">PO No
        <input v-model="model.poNo" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
      </label>
      <label class="flex flex-col text-xs font-semibold text-slate-700">Job Ref
        <input v-model="model.jobRef" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
      </label>
    </div>
  </div>

  <div class="bg-white p-5 rounded-lg mb-4">
    <div class="flex justify-between mb-3">
      <strong>Line Items</strong>
      <button class="bg-[#c2410c] text-white px-2.5 py-1 rounded-md text-sm" @click="addLine">+ Add Line</button>
    </div>
    <table class="w-full">
      <thead>
        <tr class="text-left text-sm">
          <th class="p-1">Product</th>
          <th class="p-1">Description</th>
          <th class="p-1">Unit</th>
          <th class="p-1">Qty</th>
          <th class="p-1">Unit Price</th>
          <th class="p-1">Disc %</th>
          <th class="p-1"></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(line, i) in model.items" :key="i" class="text-sm">
          <td class="p-1">
            <select v-model="line.productId" class="w-full p-1 border border-slate-300 rounded" @change="onProduct(line)">
              <option :value="null">-- custom --</option>
              <option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }}</option>
            </select>
          </td>
          <td class="p-1"><input v-model="line.description" class="w-full p-1 border border-slate-300 rounded" /></td>
          <td class="p-1"><input v-model="line.unit" class="w-[70px] p-1 border border-slate-300 rounded" /></td>
          <td class="p-1"><input v-model.number="line.quantity" type="number" class="w-[70px] p-1 border border-slate-300 rounded" /></td>
          <td class="p-1"><input v-model.number="line.unitPrice" type="number" class="w-full p-1 border border-slate-300 rounded" /></td>
          <td class="p-1"><input v-model.number="line.lineDiscountPercent" type="number" class="w-[70px] p-1 border border-slate-300 rounded" /></td>
          <td class="p-1"><button class="text-red-600" @click="removeLine(i)">x</button></td>
        </tr>
      </tbody>
    </table>
  </div>

  <div class="bg-white p-5 rounded-lg mb-4 flex gap-4 flex-wrap">
    <label class="flex flex-col text-xs font-semibold text-slate-700 flex-1 min-w-[120px]">Doc Discount %
      <input v-model.number="model.documentDiscountPercent" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
    </label>
    <label class="flex flex-col text-xs font-semibold text-slate-700 flex-1 min-w-[120px]">VAT %
      <input v-model.number="model.vatPercent" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
    </label>
    <label class="flex flex-col text-xs font-semibold text-slate-700 flex-1 min-w-[120px]">Service Charge
      <input v-model.number="model.serviceCharge" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
    </label>
    <label class="flex flex-col text-xs font-semibold text-slate-700 flex-1 min-w-[120px]">Weight Charge
      <input v-model.number="model.weightCharge" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
    </label>
    <label class="flex flex-col text-xs font-semibold text-slate-700 flex-1 min-w-[120px]">Delivery Charge
      <input v-model.number="model.deliveryCharge" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
    </label>
  </div>

  <div class="flex justify-end gap-3">
    <NuxtLink to="/quotations" class="bg-slate-200 text-slate-700 px-3.5 py-2 rounded-md no-underline">Cancel</NuxtLink>
    <button
      class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md disabled:opacity-50"
      :disabled="model.items.length === 0"
      @click="save"
    >
      Save Quotation
    </button>
  </div>
</template>
