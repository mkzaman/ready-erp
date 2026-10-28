<script setup lang="ts">
import type { Customer } from '~/types/models'

const api = useApi()
const customers = ref<Customer[]>([])
const editing = ref<Partial<Customer> | null>(null)

async function load() {
  customers.value = await api.getCustomers()
}

function newCustomer() {
  editing.value = { name: '', sites: [] }
}
function edit(c: Customer) {
  editing.value = { ...c, sites: c.sites.map((s) => ({ ...s })) }
}
function cancel() {
  editing.value = null
}
function addSite() {
  editing.value!.sites!.push({ siteName: '', address: '' })
}
function removeSite(i: number) {
  editing.value!.sites!.splice(i, 1)
}
async function save() {
  const e = editing.value!
  if (e.id) await api.updateCustomer(e.id, e)
  else await api.createCustomer(e)
  editing.value = null
  await load()
}

onMounted(load)
</script>

<template>
  <div class="flex justify-between items-center mb-4">
    <h2 class="text-[#0f2b46] text-xl font-bold">Customers</h2>
    <button class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md" @click="newCustomer">+ New</button>
  </div>

  <table class="w-full bg-white rounded-lg overflow-hidden">
    <thead>
      <tr class="bg-slate-50 text-left text-sm">
        <th class="p-2.5">Name</th>
        <th class="p-2.5">Contact</th>
        <th class="p-2.5">Phone</th>
        <th class="p-2.5">Email</th>
        <th class="p-2.5">Sites</th>
        <th class="p-2.5"></th>
      </tr>
    </thead>
    <tbody>
      <tr v-for="c in customers" :key="c.id" class="border-t border-slate-200 text-sm">
        <td class="p-2.5">{{ c.name }}</td>
        <td class="p-2.5">{{ c.contactPerson }}</td>
        <td class="p-2.5">{{ c.phone }}</td>
        <td class="p-2.5">{{ c.email }}</td>
        <td class="p-2.5">{{ c.sites.length }}</td>
        <td class="p-2.5"><button class="text-blue-600" @click="edit(c)">Edit</button></td>
      </tr>
    </tbody>
  </table>

  <div
    v-if="editing"
    class="fixed inset-0 bg-black/40 flex items-center justify-center"
    @click.self="cancel"
  >
    <div class="bg-white p-6 rounded-xl w-[640px] max-w-[90%] max-h-[90vh] overflow-auto">
      <h3 class="font-bold mb-3">{{ editing.id ? 'Edit' : 'New' }} Customer</h3>
      <div class="grid grid-cols-2 gap-3">
        <label class="flex flex-col text-xs font-semibold text-slate-700">Name
          <input v-model="editing.name" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Contact Person
          <input v-model="editing.contactPerson" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Phone
          <input v-model="editing.phone" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">Email
          <input v-model="editing.email" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700">VAT/BIN
          <input v-model="editing.vatBin" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
        <label class="flex flex-col text-xs font-semibold text-slate-700 col-span-2">Billing Address
          <input v-model="editing.billingAddress" class="mt-1 p-1.5 border border-slate-300 rounded-md font-normal" />
        </label>
      </div>

      <div class="mt-4 border-t border-slate-200 pt-4">
        <div class="flex justify-between items-center mb-2">
          <strong>Project Sites</strong>
          <button class="bg-[#c2410c] text-white px-2.5 py-1 rounded-md text-sm" @click="addSite">+ Add Site</button>
        </div>
        <div v-for="(s, i) in editing.sites" :key="i" class="flex gap-2 mb-2">
          <input v-model="s.siteName" placeholder="Site name" class="flex-1 p-1.5 border border-slate-300 rounded-md" />
          <input v-model="s.address" placeholder="Address" class="flex-1 p-1.5 border border-slate-300 rounded-md" />
          <button class="bg-slate-200 text-slate-700 px-2.5 py-1 rounded-md text-sm" @click="removeSite(i)">x</button>
        </div>
      </div>

      <div class="flex justify-end gap-3 mt-5">
        <button class="bg-slate-200 text-slate-700 px-3.5 py-2 rounded-md" @click="cancel">Cancel</button>
        <button class="bg-[#c2410c] text-white px-3.5 py-2 rounded-md" @click="save">Save</button>
      </div>
    </div>
  </div>
</template>
