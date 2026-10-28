<script setup lang="ts">
const { username, logout } = useAuth()

const links = [
  { to: '/dashboard', label: 'Dashboard' },
  { to: '/products', label: 'Products' },
  { to: '/customers', label: 'Customers' },
  { to: '/quotations', label: 'Quotations' },
  { to: '/invoices', label: 'Invoices' },
  { to: '/dispatches', label: 'Dispatches' }
]

function onLogout() {
  logout()
  navigateTo('/login')
}
</script>

<template>
  <div class="flex min-h-screen">
    <aside class="w-56 bg-[#0f2b46] text-slate-300 flex flex-col print:hidden">
      <div class="px-5 py-5 text-white font-bold text-lg border-b border-[#1e3a5f]">E&G ERP</div>
      <nav class="flex flex-col p-2 gap-1">
        <NuxtLink
          v-for="l in links"
          :key="l.to"
          :to="l.to"
          class="px-4 py-2.5 rounded-md text-sm hover:bg-[#1e3a5f] hover:text-white"
          active-class="bg-[#1e3a5f] text-white"
        >
          {{ l.label }}
        </NuxtLink>
      </nav>
    </aside>
    <main class="flex-1 bg-slate-100 flex flex-col">
      <header class="flex justify-end items-center gap-4 px-6 py-3 bg-white border-b border-slate-200 print:hidden">
        <span class="text-sm text-slate-600">{{ username }}</span>
        <button
          class="bg-[#c2410c] text-white px-3.5 py-1.5 rounded-md text-sm"
          @click="onLogout"
        >
          Logout
        </button>
      </header>
      <div class="p-6">
        <slot />
      </div>
    </main>
  </div>
</template>
