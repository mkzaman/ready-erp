<script setup lang="ts">
import type { QuotationListItem } from '~/types/models'

const api = useApi()
const quotations = ref<QuotationListItem[]>([])
const message = ref('')

async function load() {
  quotations.value = await api.getQuotations()
}

async function convert(q: QuotationListItem) {
  const res = await api.convertQuotation(q.id)
  message.value = `Created Invoice #${res.invoiceId} and Dispatch #${res.dispatchId}.`
  await load()
}

function badgeClass(status: string) {
  const map: Record<string, string> = {
    Accepted: 'bg-green-100 text-green-800',
    Draft: 'bg-slate-200 text-slate-600',
    Sent: 'bg-blue-100 text-blue-800'
  }
  return map[status] || 'bg-slate-200 text-slate-600'
}

onMounted(load)
</script>

<template>
  <div class="flex justify-between items-center mb-4">
    <h2 class="text-[#0f2b46] text-xl font-bold">Quotations</h2>
    <NuxtLink to="/quotations/new" class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md no-underline">
      + New Quotation
    </NuxtLink>
  </div>

  <table class="w-full bg-white rounded-lg overflow-hidden">
    <thead>
      <tr class="bg-slate-50 text-left text-sm">
        <th class="p-2.5">Number</th>
        <th class="p-2.5">Customer</th>
        <th class="p-2.5">Status</th>
        <th class="p-2.5">Grand Total</th>
        <th class="p-2.5"></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="q in quotations" :key="q.id" class="border-t border-slate-200 text-sm">
        <td class="p-2.5">{{ q.number }}</td>
        <td class="p-2.5">{{ q.customer }}</td>
        <td class="p-2.5">
          <span class="px-2.5 py-0.5 rounded-full text-xs font-semibold" :class="badgeClass(q.status)">
            {{ q.status }}
          </span>
        </td>
        <td class="p-2.5">{{ formatNumber(q.totals.grandTotal) }}</td>
        <td class="p-2.5">
          <button v-if="q.status !== 'Accepted'" class="text-blue-600" @click="convert(q)">
            Convert to Invoice+Dispatch
          </button>
          <span v-else class="text-slate-400">Converted</span>
        </td>
      </tr>
    </tbody>
  </table>

  <p v-if="message" class="mt-4 text-green-800">{{ message }}</p>
</template>
