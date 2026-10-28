<script setup lang="ts">
const api = useApi()
const invoices = ref<any[]>([])
const current = ref<any | null>(null)

async function view(id: number) {
  current.value = await api.getInvoice(id)
}
function print() {
  window.print()
}

onMounted(async () => {
  invoices.value = await api.getInvoices()
})
</script>

<template>
  <h2 class="text-[#0f2b46] text-xl font-bold mb-4 print:hidden">Invoices</h2>

  <table class="w-full bg-white rounded-lg overflow-hidden print:hidden">
    <thead>
      <tr class="bg-slate-50 text-left text-sm">
        <th class="p-2.5">Number</th>
        <th class="p-2.5">Customer</th>
        <th class="p-2.5">Grand Total</th>
        <th class="p-2.5">Status</th>
        <th class="p-2.5"></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="i in invoices" :key="i.id" class="border-t border-slate-200 text-sm">
        <td class="p-2.5">{{ i.number }}</td>
        <td class="p-2.5">{{ i.customer }}</td>
        <td class="p-2.5">{{ formatNumber(i.totals.grandTotal) }}</td>
        <td class="p-2.5">{{ i.isVoided ? 'Voided' : 'Active' }}</td>
        <td class="p-2.5"><button class="text-blue-600" @click="view(i.id)">View / Print</button></td>
      </tr>
    </tbody>
  </table>

  <div
    v-if="current"
    class="fixed inset-0 bg-black/40 flex items-start justify-center overflow-auto p-8 print:static print:bg-transparent print:p-0 print:block"
    @click.self="current = null"
  >
    <div class="bg-white rounded-lg w-[800px] print:w-full">
      <div class="flex justify-end gap-3 p-4 print:hidden">
        <button class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md" @click="print">Print</button>
        <button class="bg-slate-200 text-slate-700 px-3.5 py-2 rounded-md" @click="current = null">Close</button>
      </div>

      <div class="p-8">
        <div class="flex justify-between border-b-2 border-[#0f2b46] pb-4">
          <div>
            <h1 class="m-0 text-[#0f2b46] text-2xl font-bold">E&G Engineering Ltd.</h1>
            <p class="text-sm text-slate-500">Inventory-Based Invoicing System</p>
          </div>
          <div class="text-right">
            <h2 class="m-0 text-[#c2410c] text-xl font-bold">INVOICE</h2>
            <p class="text-sm">{{ current.invoice.number }}</p>
            <p class="text-sm">{{ formatDate(current.invoice.date) }}</p>
          </div>
        </div>

        <div class="flex justify-between my-6 text-sm">
          <div>
            <strong>{{ current.invoice.customer?.name || current.invoice.customerNameOverride }}</strong>
            <p>{{ current.invoice.customer?.billingAddress }}</p>
            <p>{{ current.invoice.customer?.contactPerson }} {{ current.invoice.customer?.phone }}</p>
          </div>
          <div>
            <p><b>Work Ref:</b> {{ current.invoice.workRef }}</p>
            <p><b>Project Site:</b> {{ current.invoice.projectSite }}</p>
            <p><b>PO No:</b> {{ current.invoice.poNo }}</p>
            <p><b>Job Ref:</b> {{ current.invoice.jobRef }}</p>
          </div>
        </div>

        <table class="w-full border border-slate-200 text-sm">
          <thead>
            <tr class="bg-[#0f2b46] text-white text-left">
              <th class="p-2">SL</th>
              <th class="p-2">Description</th>
              <th class="p-2">UoM</th>
              <th class="p-2">Qty</th>
              <th class="p-2">Unit Price</th>
              <th class="p-2">Total</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(it, idx) in current.invoice.items" :key="it.id" class="border-t border-slate-200">
              <td class="p-2">{{ idx + 1 }}</td>
              <td class="p-2">{{ it.description }}</td>
              <td class="p-2">{{ it.unit }}</td>
              <td class="p-2">{{ it.quantity }}</td>
              <td class="p-2">{{ formatNumber(it.unitPrice) }}</td>
              <td class="p-2">{{ formatNumber(it.quantity * it.unitPrice) }}</td>
            </tr>
          </tbody>
        </table>

        <div class="mt-4 ml-auto w-[300px] text-sm">
          <p class="flex justify-between my-1"><span>Sub-Total</span><span>{{ formatNumber(current.totals.subTotal) }}</span></p>
          <p v-if="current.totals.charges" class="flex justify-between my-1"><span>Charges</span><span>{{ formatNumber(current.totals.charges) }}</span></p>
          <p v-if="current.invoice.vatPercent" class="flex justify-between my-1"><span>Add VAT ({{ current.invoice.vatPercent }}%)</span><span>{{ formatNumber(current.totals.vat) }}</span></p>
          <p class="flex justify-between border-t-2 border-[#0f2b46] pt-2 font-bold"><span>Grand Total</span><span>{{ formatNumber(current.totals.grandTotal) }}</span></p>
          <p class="italic text-xs text-slate-600 mt-1">In Word: {{ current.totals.grandTotalInWords }}</p>
        </div>

        <div class="flex justify-between mt-12">
          <span class="border-t border-slate-700 pt-2 w-[180px] text-center text-sm">Prepared By</span>
          <span class="border-t border-slate-700 pt-2 w-[180px] text-center text-sm">Received By</span>
        </div>
      </div>
    </div>
  </div>
</template>
