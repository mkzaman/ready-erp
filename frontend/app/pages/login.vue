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
  <div
    class="flex min-h-screen items-center justify-center bg-on-background p-space-md font-body-md text-body-md"
  >
    <div class="w-full max-w-sm">
      <div class="mb-space-lg flex items-center gap-space-sm">
        <span
          class="flex size-11 shrink-0 items-center justify-center rounded-xl bg-primary-container text-on-primary"
        >
          <Icon name="material-symbols:monitor-heart-outline" class="size-6" />
        </span>
        <div>
          <h1 class="font-headline-md text-headline-md text-surface-bright">E&amp;G Engineering</h1>
          <p class="font-label-md text-label-md text-outline-variant">
            Inventory, Invoicing &amp; Quotations
          </p>
        </div>
      </div>

      <form
        class="flex flex-col gap-space-md rounded-xl bg-surface-container-lowest p-space-xl elevation-3"
        @submit.prevent="submit"
      >
        <div>
          <h2 class="font-headline-sm text-headline-sm text-on-surface">Sign in</h2>
          <p class="mt-0.5 font-body-sm text-body-sm text-on-surface-variant">
            Use your operations account to continue.
          </p>
        </div>

        <FormField
          v-model="username"
          label="Username"
          icon="person"
          autocomplete="username"
          required
        />

        <FormField
          v-model="password"
          label="Password"
          type="password"
          icon="lock"
          autocomplete="current-password"
          required
        />

        <p
          v-if="error"
          class="flex items-center gap-1.5 rounded-xl bg-error-container px-space-sm py-2 font-body-sm text-body-sm text-on-error-container"
          role="alert"
        >
          <Icon name="material-symbols:error-outline" class="size-4 shrink-0" />
          {{ error }}
        </p>

        <AppButton type="submit" :loading="loading" class="w-full">
          {{ loading ? 'Signing in...' : 'Sign in' }}
        </AppButton>

        <p class="text-center font-label-sm text-label-sm text-on-surface-variant">
          Demo login: admin / admin
        </p>
      </form>
    </div>
  </div>
</template>
