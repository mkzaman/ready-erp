<script setup lang="ts">
definePageMeta({ layout: false })

const { login } = useAuth()
const username = ref('admin')
const password = ref('admin')
const loading = ref(false)
const error = ref('')

async function submit() {
  loading.value = true
  error.value = ''
  try {
    await login(username.value, password.value)
    await navigateTo('/dashboard')
  } catch {
    error.value = 'Invalid username or password'
    loading.value = false
  }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-[#0f2b46]">
    <form class="bg-white p-10 rounded-xl w-[340px] flex flex-col shadow-2xl" @submit.prevent="submit">
      <h1 class="m-0 text-[#0f2b46] text-2xl font-bold">E&G Engineering</h1>
      <p class="text-slate-500 text-sm mt-1 mb-6">Inventory, Invoicing & Quotation System</p>

      <label class="text-xs font-semibold text-slate-700 mt-3">Username</label>
      <input
        v-model="username"
        autocomplete="username"
        class="p-2.5 border border-slate-300 rounded-md mt-1"
      />

      <label class="text-xs font-semibold text-slate-700 mt-3">Password</label>
      <input
        v-model="password"
        type="password"
        autocomplete="current-password"
        class="p-2.5 border border-slate-300 rounded-md mt-1"
      />

      <p v-if="error" class="text-red-600 text-sm mt-3">{{ error }}</p>

      <button
        type="submit"
        :disabled="loading"
        class="mt-6 p-2.5 bg-[#c2410c] text-white rounded-md font-semibold disabled:opacity-60"
      >
        {{ loading ? 'Signing in...' : 'Sign In' }}
      </button>
      <p class="text-slate-400 text-xs text-center mt-4">Demo login: admin / admin</p>
    </form>
  </div>
</template>
