<script setup lang="ts">
import type { Customer, CustomerSite } from '~/types/models'
import type { TableColumn } from '~/types/table'

const api = useApi()
const toast = useToast()

const customers = ref<Customer[]>([])
const loading = ref(true)
const search = ref('')
const page = ref(1)
const pageSize = ref(25)

const columns: TableColumn[] = [
  { key: 'name', label: 'Customer', width: 'w-80' },
  { key: 'contactPerson', label: 'Contact', hideBelow: 'md' },
  { key: 'phone', label: 'Phone', hideBelow: 'lg' },
  { key: 'vatBin', label: 'VAT / BIN', hideBelow: 'xl' },
  { key: 'sites', label: 'Sites', align: 'right', numeric: true }
]

// The customers endpoint is not paged server-side, so paging is applied
// client-side over the fetched list.
const filtered = computed(() => {
  const term = search.value.trim().toLowerCase()
  if (!term) return customers.value
  return customers.value.filter(
    (c) =>
      c.name.toLowerCase().includes(term) ||
      (c.contactPerson ?? '').toLowerCase().includes(term) ||
      (c.email ?? '').toLowerCase().includes(term) ||
      (c.phone ?? '').toLowerCase().includes(term)
  )
})

const paged = computed(() =>
  filtered.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value)
)

watch(search, () => (page.value = 1))

async function load() {
  loading.value = true
  try {
    customers.value = await api.getCustomers()
  } catch (error) {
    toast.error(error)
    customers.value = []
  } finally {
    loading.value = false
  }
}

onMounted(load)

// ---------------------------------------------------------------------------
// Editor
// ---------------------------------------------------------------------------
interface CustomerForm {
  name: string
  contactPerson: string | null
  phone: string | null
  email: string | null
  vatBin: string | null
  billingAddress: string | null
  sites: CustomerSite[]
}

const EMPTY: CustomerForm = {
  name: '',
  contactPerson: null,
  phone: null,
  email: null,
  vatBin: null,
  billingAddress: null,
  sites: []
}

const open = ref(false)
const saving = ref(false)
const editingId = ref<number | null>(null)
const form = ref<CustomerForm>({ ...EMPTY, sites: [] })
const formError = ref('')

function openCreate() {
  editingId.value = null
  form.value = { ...EMPTY, sites: [] }
  formError.value = ''
  open.value = true
}

function openEdit(customer: Customer) {
  editingId.value = customer.id
  form.value = {
    name: customer.name,
    contactPerson: customer.contactPerson,
    phone: customer.phone,
    email: customer.email,
    vatBin: customer.vatBin,
    billingAddress: customer.billingAddress,
    // Clone so cancelling does not mutate the row in the table.
    sites: customer.sites.map((s) => ({ ...s }))
  }
  formError.value = ''
  open.value = true
}

function addSite() {
  form.value.sites.push({ siteName: '', address: '' })
}

function removeSite(index: number) {
  form.value.sites.splice(index, 1)
}

async function save() {
  formError.value = ''

  if (!form.value.name.trim()) {
    formError.value = 'Customer name is required.'
    return
  }

  // The API recreates all sites on update; drop blank rows so empty repeater
  // entries are not persisted.
  const payload = {
    ...form.value,
    sites: form.value.sites.filter((s) => s.siteName.trim())
  }

  saving.value = true
  try {
    if (editingId.value) await api.updateCustomer(editingId.value, payload)
    else await api.createCustomer(payload)

    toast.success(editingId.value ? `${payload.name} updated` : `${payload.name} created`)
    open.value = false
    await load()
  } catch (error: any) {
    formError.value = error?.data?.message || error?.message || 'Could not save the customer.'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <PageHeader
    title="Customers"
    description="Billing details and project sites used across quotations and invoices."
  >
    <template #actions>
      <AppButton icon="add" @click="openCreate">New customer</AppButton>
    </template>
  </PageHeader>

  <TableToolbar v-model:search="search" search-placeholder="Search name, contact, phone or email..." />

  <DataTable
    :columns="columns"
    :rows="paged"
    :loading="loading"
    empty-title="No customers found"
    empty-description="Add a customer to start raising quotations."
    empty-icon="groups"
  >
    <template #cell-name="{ row }">
      <div class="min-w-0">
        <p class="truncate font-label-lg text-label-lg text-on-surface">{{ row.name }}</p>
        <p class="truncate font-label-sm text-label-sm text-on-surface-variant">
          {{ row.email || row.billingAddress || '—' }}
        </p>
      </div>
    </template>

    <template #cell-contactPerson="{ value }">
      {{ value || '—' }}
    </template>

    <template #cell-phone="{ value }">
      {{ value || '—' }}
    </template>

    <template #cell-vatBin="{ value }">
      {{ value || '—' }}
    </template>

    <template #cell-sites="{ row }">
      <StatusBadge
        :status="`${row.sites.length} site${row.sites.length === 1 ? '' : 's'}`"
        plain
        :tone="row.sites.length ? 'info' : 'neutral'"
      />
    </template>

    <template #actions="{ row }">
      <AppButton variant="ghost" size="icon" icon="edit" title="Edit" @click="openEdit(row)" />
    </template>

    <template #empty-action>
      <AppButton icon="add" @click="openCreate">New customer</AppButton>
    </template>
  </DataTable>

  <TablePagination v-model:page="page" v-model:page-size="pageSize" :total="filtered.length" />

  <AppModal
    :open="open"
    variant="drawer"
    :title="editingId ? 'Edit customer' : 'New customer'"
    :description="editingId ? form.name : 'Add a billing account and its project sites.'"
    @close="open = false"
  >
    <div class="flex flex-col gap-space-lg">
      <div class="grid grid-cols-1 gap-space-md sm:grid-cols-2">
        <FormField v-model="form.name" label="Customer name" required class="sm:col-span-2" />
        <FormField v-model="form.contactPerson" label="Contact person" />
        <FormField v-model="form.phone" label="Phone" type="tel" />
        <FormField v-model="form.email" label="Email" type="email" />
        <FormField v-model="form.vatBin" label="VAT / BIN" />
        <FormField
          v-model="form.billingAddress"
          label="Billing address"
          as="textarea"
          :rows="2"
          class="sm:col-span-2"
        />
      </div>

      <div class="border-t border-surface-container pt-space-md">
        <div class="mb-space-sm flex items-center justify-between gap-space-md">
          <div>
            <h3 class="font-headline-sm text-headline-sm text-on-surface">Project sites</h3>
            <p class="font-label-sm text-label-sm text-on-surface-variant">
              Delivery destinations selectable on documents.
            </p>
          </div>
          <AppButton variant="secondary" size="sm" icon="add" @click="addSite">Add site</AppButton>
        </div>

        <div v-if="form.sites.length === 0" class="rounded-xl bg-surface-container-low p-space-md">
          <p class="font-body-sm text-body-sm text-on-surface-variant">
            No sites yet. Billing address will be used instead.
          </p>
        </div>

        <div v-else class="flex flex-col gap-space-sm">
          <div
            v-for="(site, index) in form.sites"
            :key="index"
            class="flex items-end gap-space-sm rounded-xl bg-surface-container-low p-space-sm"
          >
            <FormField v-model="site.siteName" label="Site name" class="flex-1" />
            <FormField v-model="site.address" label="Address" class="flex-1" />
            <AppButton
              variant="ghost"
              size="icon"
              icon="delete"
              title="Remove site"
              class="mb-1"
              @click="removeSite(index)"
            />
          </div>
        </div>
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
        {{ editingId ? 'Save changes' : 'Create customer' }}
      </AppButton>
    </template>
  </AppModal>
</template>
