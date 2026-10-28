<script setup lang="ts">
const api = useApi()
const dispatches = ref<any[]>([])
const current = ref<any | null>(null)
const message = ref('')

async function load() {
  dispatches.value = await api.getDispatches()
}
async function view(id: number) {
  current.value = await api.getDispatch(id)
}
async function issue(id: number) {
  const res = await api.issueDispatch(id)
  message.value = res.warnings?.length
    ? `Issued with stock warnings: ${res.warnings.join('; ')}`
    : 'Dispatch issued. Stock reduced.'
  await load()
}
function print() {
  window.print()
}

onMounted(load)
</script>

<template>
  <h2 class="text-[#0f2b46] text-xl font-bold mb-4 print:hidden">Dispatch Notes</h2>

  <table class="w-full bg-white rounded-lg overflow-hidden print:hidden">
    <thead>
      <tr class="bg-slate-50 text-left text-sm">
        <th class="p-2.5">Number</th>
        <th class="p-2.5">Customer</th>
        <th class="p-2.5">Lines</th>
        <th class="p-2.5">Status</th>
        <th class="p-2.5"></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="d in dispatches" :key="d.id" class="border-t border-slate-200 text-sm">
        <td class="p-2.5">{{ d.number }}</td>
        <td class="p-2.5">{{ d.customer }}</td>
        <td class="p-2.5">{{ d.lineCount }}</td>
        <td class="p-2.5">{{ d.isIssued ? 'Issued' : 'Pending' }}</td>
        <td class="p-2.5">
          <button class="text-blue-600 mr-4" @click="view(d.id)">View / Print</button>
          <button v-if="!d.isIssued" class="text-[#c2410c]" @click="issue(d.id)">Issue (reduce stock)</button>
        </td>
      </tr>
    </tbody>
  </table>

  <p v-if="message" class="mt-4 text-green-800 print:hidden">{{ message }}</p>

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
          <div><h1 class="m-0 text-[#0f2b46] text-2xl font-bold">E&G Engineering Ltd.</h1></div>
          <div class="text-right">
            <h2 class="m-0 text-[#c2410c] text-xl font-bold">DISPATCH NOTE</h2>
            <p class="text-sm">{{ current.dispatch.number }}</p>
            <p class="text-sm">{{ formatDate(current.dispatch.date) }}</p>
          </div>
        </div>

        <div class="flex justify-between my-6 text-sm">
          <div>
            <strong>{{ current.dispatch.customer?.name || current.dispatch.customerNameOverride }}</strong>
            <p>{{ current.dispatch.customer?.billingAddress }}</p>
          </div>
          <div>
            <p><b>Work Ref:</b> {{ current.dispatch.workRef }}</p>
            <p><b>Project Site:</b> {{ current.dispatch.projectSite }}</p>
            <p><b>PO No:</b> {{ current.dispatch.poNo }}</p>
          </div>
        </div>

        <table class="w-full border border-slate-200 text-sm">
          <thead>
            <tr class="bg-[#0f2b46] text-white text-left">
              <th class="p-2">SL</th>
              <th class="p-2">Description</th>
              <th class="p-2">UoM</th>
              <th class="p-2">Qty</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(it, idx) in current.dispatch.items" :key="it.id" class="border-t border-slate-200">
              <td class="p-2">{{ idx + 1 }}</td>
              <td class="p-2">{{ it.description }}</td>
              <td class="p-2">{{ it.unit }}</td>
              <td class="p-2">{{ it.quantity }}</td>
            </tr>
          </tbody>
        </table>

        <div class="flex justify-between mt-12">
          <span class="border-t border-slate-700 pt-2 w-[180px] text-center text-sm">Prepared By</span>
          <span class="border-t border-slate-700 pt-2 w-[180px] text-center text-sm">Received By</span>
        </div>
      </div>
    </div>
  </div>
</template>
