<script setup lang="ts">
/**
 * Fixed topbar. Hosts the mobile navigation trigger and global product search.
 *
 * The StockPulse mock also shows a warehouse status pill and a notification
 * bell; both are omitted until there is real data behind them (warehouses
 * arrive with the backend work, notifications do not exist at all).
 */
const navOpen = defineModel<boolean>('navOpen', { default: false })

const { username } = useAuth()
const router = useRouter()

const query = ref('')
const searchEl = ref<HTMLInputElement | null>(null)

const initials = computed(() => (username.value ?? '?').slice(0, 2).toUpperCase())

function submit() {
  const q = query.value.trim()
  if (!q) return
  router.push({ path: '/products', query: { q } })
}

function onKeydown(event: KeyboardEvent) {
  if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') {
    event.preventDefault()
    searchEl.value?.focus()
  }
}

onMounted(() => document.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => document.removeEventListener('keydown', onKeydown))
</script>

<template>
  <header
    class="fixed inset-x-0 top-0 z-30 flex h-16 items-center gap-space-md border-b border-surface-container bg-surface/80 px-space-md backdrop-blur-xl lg:left-64 lg:px-margin print:hidden"
  >
    <button
      type="button"
      class="rounded-lg p-1.5 text-on-surface-variant transition-colors hover:bg-surface-container hover:text-on-surface lg:hidden"
      aria-label="Open navigation"
      @click="navOpen = true"
    >
      <Icon name="material-symbols:menu" class="size-6" />
    </button>

    <form class="relative w-full max-w-md" role="search" @submit.prevent="submit">
      <Icon
        name="material-symbols:search"
        class="pointer-events-none absolute left-3 top-1/2 size-5 -translate-y-1/2 text-on-surface-variant"
      />
      <input
        ref="searchEl"
        v-model="query"
        type="search"
        placeholder="Search products..."
        aria-label="Search products"
        class="h-10 w-full rounded-xl bg-surface-container-low pl-10 pr-16 font-body-sm text-body-sm text-on-surface transition-all placeholder:text-on-surface-variant focus:bg-surface-container-lowest focus:shadow-[0_0_0_2px_rgb(37_99_235_/_0.4)] focus:outline-none"
      >
      <kbd
        class="pointer-events-none absolute right-2.5 top-1/2 hidden -translate-y-1/2 rounded-lg border border-surface-container-highest px-1.5 py-0.5 font-label-sm text-label-sm text-on-surface-variant sm:block"
      >
        Ctrl K
      </kbd>
    </form>

    <div class="ml-auto flex items-center gap-space-sm">
      <span class="hidden font-label-md text-label-md text-on-surface-variant sm:block">
        {{ username }}
      </span>
      <span
        class="flex size-8 shrink-0 items-center justify-center rounded-full bg-primary font-label-sm text-label-sm text-on-primary"
      >
        {{ initials }}
      </span>
    </div>
  </header>
</template>
