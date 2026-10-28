<script setup lang="ts">
import type { DashboardData } from '~/types/models'

const api = useApi()
const data = ref<DashboardData | null>(null)

const cards = computed(() => {
  if (!data.value) return []
  return [
    { label: 'Products', value: data.value.productCount },
    { label: 'Customers', value: data.value.customerCount },
    { label: 'Quotations', value: data.value.quotationCount },
    { label: 'Invoices', value: data.value.invoiceCount },
    { label: 'Dispatches', value: data.value.dispatchCount }
  ]
})

onMounted(async () => {
  data.value = await api.getDashboard()
})
</script>

<template>
  <h2 class="text-[#0f2b46] text-xl font-bold mb-4">Dashboard</h2>

  <div v-if="data" class="flex gap-4 flex-wrap mb-8">
    <div
      v-for="c in cards"
      :key="c.label"
      class="bg-white rounded-xl p-6 min-w-[130px] flex flex-col shadow-sm text-slate-500 text-sm"
    >
      <span class="text-3xl font-bold text-[#0f2b46]">{{ c.value }}</span>
      {{ c.label }}
    </div>
  </div>

  <template v-if="data">
    <h3 class="text-[#0f2b46] font-semibold mb-2">Low Stock</h3>
    <p v-if="data.lowStock.length === 0" class="text-slate-400">No low-stock items.</p>
    <table v-else class="w-full bg-white rounded-lg overflow-hidden">
      <thead>
        <tr class="bg-slate-50 text-left text-sm">
          <th class="p-2.5">Part No</th>
          <th class="p-2.5">Name</th>
          <th class="p-2.5">Stock</th>
          <th class="p-2.5">Threshold</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in data.lowStock" :key="item.partNumber" class="border-t border-slate-200 text-sm">
          <td class="p-2.5">{{ item.partNumber }}</td>
          <td class="p-2.5">{{ item.name }}</td>
          <td class="p-2.5 text-red-600 font-semibold">{{ item.stockQuantity }}</td>
          <td class="p-2.5">{{ item.lowStockThreshold }}</td>
        </tr>
      </tbody>
    </table>
  </template>
</template>
