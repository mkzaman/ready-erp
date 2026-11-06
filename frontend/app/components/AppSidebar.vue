<script setup lang="ts">
/**
 * Fixed application sidebar. Collapses to an off-canvas drawer below `lg`,
 * driven by the `open` model so the topbar hamburger can control it.
 */
const open = defineModel<boolean>('open', { default: false })

const { username, logout } = useAuth()
const route = useRoute()

const links = [
  { to: '/dashboard', label: 'Dashboard', icon: 'dashboard' },
  { to: '/products', label: 'Products', icon: 'inventory-2' },
  { to: '/customers', label: 'Customers', icon: 'groups' },
  { to: '/quotations', label: 'Quotations', icon: 'request-quote' },
  { to: '/invoices', label: 'Invoices', icon: 'receipt-long' },
  { to: '/dispatches', label: 'Dispatches', icon: 'local-shipping' }
]

const initials = computed(() => (username.value ?? '?').slice(0, 2).toUpperCase())

function onLogout() {
  logout()
  navigateTo('/login')
}

// Close the drawer whenever navigation occurs on small screens.
watch(() => route.fullPath, () => { open.value = false })
</script>

<template>
  <!-- Scrim, small screens only -->
  <Transition
    enter-active-class="transition-opacity duration-200"
    leave-active-class="transition-opacity duration-200"
    enter-from-class="opacity-0"
    leave-to-class="opacity-0"
  >
    <div
      v-if="open"
      class="fixed inset-0 z-40 bg-on-background/40 backdrop-blur-[2px] lg:hidden print:hidden"
      @click="open = false"
    />
  </Transition>

  <aside
    class="fixed left-0 top-0 z-50 flex h-screen w-64 flex-col bg-on-background text-surface transition-transform duration-200 lg:translate-x-0 print:hidden"
    :class="open ? 'translate-x-0' : '-translate-x-full'"
  >
    <!-- Brand -->
    <div class="flex h-16 shrink-0 items-center gap-space-sm bg-inverse-surface px-space-md">
      <span
        class="flex size-9 shrink-0 items-center justify-center rounded-xl bg-primary-container text-on-primary"
      >
        <Icon name="material-symbols:monitor-heart-outline" class="size-5" />
      </span>
      <div class="min-w-0">
        <p class="truncate font-headline-sm text-headline-sm text-surface-bright">E&amp;G ERP</p>
        <p class="truncate font-label-sm text-label-sm text-outline-variant">Engineering Ops</p>
      </div>

      <button
        type="button"
        class="ml-auto rounded-lg p-1 text-outline-variant transition-colors hover:text-surface-bright lg:hidden"
        aria-label="Close navigation"
        @click="open = false"
      >
        <Icon name="material-symbols:close" class="size-5" />
      </button>
    </div>

    <!-- Navigation -->
    <nav class="flex-1 space-y-1 overflow-y-auto px-space-sm py-space-md">
      <NuxtLink
        v-for="link in links"
        :key="link.to"
        :to="link.to"
        class="flex items-center gap-space-sm rounded-xl px-space-md py-space-sm font-label-lg text-label-lg text-outline-variant transition-colors hover:bg-inverse-surface hover:text-surface-bright"
        active-class="bg-primary-container font-semibold text-on-primary hover:bg-primary-container hover:text-on-primary"
      >
        <Icon :name="`material-symbols:${link.icon}`" class="size-5 shrink-0" />
        {{ link.label }}
      </NuxtLink>
    </nav>

    <!-- Account -->
    <div class="shrink-0 bg-inverse-surface/60 p-space-sm">
      <div class="flex items-center gap-space-sm rounded-xl p-space-sm">
        <span
          class="flex size-8 shrink-0 items-center justify-center rounded-full bg-secondary font-label-sm text-label-sm text-on-secondary"
        >
          {{ initials }}
        </span>
        <div class="min-w-0 flex-1">
          <p class="truncate font-label-lg text-label-lg text-surface-bright">{{ username }}</p>
          <p class="truncate font-label-sm text-label-sm text-outline-variant">Signed in</p>
        </div>
        <button
          type="button"
          class="shrink-0 rounded-lg p-1.5 text-outline-variant transition-colors hover:bg-inverse-surface hover:text-surface-bright"
          aria-label="Log out"
          title="Log out"
          @click="onLogout"
        >
          <Icon name="material-symbols:logout" class="size-5" />
        </button>
      </div>
    </div>
  </aside>
</template>
