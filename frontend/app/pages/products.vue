<script setup lang="ts">
import type { Product } from '~/types/models'

const api = useApi()
const products = ref<Product[]>([])
const editing = ref<Partial<Product> | null>(null)
const search = ref('')

async function load() {
  products.value = await api.getProducts(search.value)
}

function newProduct() {
  editing.value = {
    type: 'Product', unit: 'Pcs', buyingPrice: 0, sellingPrice: 0,
    stockQuantity: 0, lowStockThreshold: null
  }
}
function edit(p: Product) {
  editing.value = { ...p }
}
function cancel() {
  editing.value = null
}
async function save() {
  const e = editing.value!
  if (e.id) await api.updateProduct(e.id, e)
  else await api.createProduct(e)
  editing.value = null
  await load()
}

function isLow(p: Product) {
  return p.lowStockThreshold != null && p.stockQuantity <= p.lowStockThreshold
}

onMounted(load)
</script>

<template>
  <div class="flex justify-between items-center">
    <h2 class="text-[#0f2b46] text-xl font-bold">Products & Services</h2>
    <button class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md" @click="newProduct">+ New</button>
  </div>

  <input
    v-model="search"
    placeholder="Search by part no, name, category..."
    class="w-full p-2.5 my-4 border border-slate-300 rounded-md"
    @input="load"
  />

  <table class="w-full bg-white rounded-lg overflow-hidden">
    <thead>
      <tr class="bg-slate-50 text-left text-sm">
        <th class="p-2.5">Part No</th>
        <th class="p-2.5">Name</th>
        <th class="p-2.5">Category</th>
        <th class="p-2.5">Type</th>
        <th class="p-2.5">Unit</th>
        <th class="p-2.5">Selling</th>
        <th class="p-2.5">Stock</th>
        <th class="p-2.5"></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="p in products" :key="p.id" class="border-t border-slate-200 text-sm">
        <td class="p-2.5">{{ p.partNumber }}</td>
        <td class="p-2.5">{{ p.name }}</td>
        <td class="p-2.5">{{ p.category }}</td>
        <td class="p-2.5">{{ p.type }}</td>
        <td class="p-2.5">{{ p.unit }}</td>
        <td class="p-2.5">{{ formatNumber(p.sellingPrice) }}</td>
        <td class="p-2.5" :class="{ 'text-red-600 font-semibold': isLow(p) }">
          {{ p.type === 'Service' ? '-' : p.stockQuantity }}
        </td>
        <td class="p-2.5">
          <button class="text-blue-600" @click="edit(p)">Edit</button>
        </td>
      </tr>
    </tbody>
  </table>

  <div
    v-if="editing"
    class="fixed inset-0 bg-black/40 flex items-center justify-center"
    @click.self="cancel"
  >
    <div class="bg-white p-6 rounded-xl w-[640px] max-w-[90%]">
      <h3 class="font-bold mb-3">{{ editing.id ? 'Edit' : 'New' }} Product</h3>
      <div class="grid grid-cols-2 gap-3">
        <label class="flex flex-col text-xs font-semibold text-slate-700">Part Number
          <input v-model="editing.partNumber" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Name
          <input v-model="editing.name" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Category
          <input v-model="editing.category" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Unit
          <input v-model="editing.unit" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Type
          <select v-model="editing.type" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal">
            <option>Product</option>
            <option>Service</option>
          </select>
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Buying Price
          <input v-model.number="editing.buyingPrice" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Selling Price
          <input v-model.number="editing.sellingPrice" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Stock Qty
          <input v-model.number="editing.stockQuantity" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Low Stock Threshold
          <input v-model.number="editing.lowStockThreshold" type="number" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
      </div>
      <div class="flex justify-end gap-3 mt-5">
        <button class="bg-slate-200 text-slate-700 px-3.5 py-2 rounded-md" @click="cancel">Cancel</button>
        <button class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md" @click="save">Save</button>
      </div>
    </div>
  </div>
</template>
