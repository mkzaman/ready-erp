import type { LoginResponse } from '~/types/models'

const TOKEN_KEY = 'egel_token'
const USER_KEY = 'egel_user'

export function useAuth() {
  const config = useRuntimeConfig()
  const token = useState<string | null>('auth_token', () => null)
  const username = useState<string | null>('auth_user', () => null)

  // Hydrate from localStorage (client-only, SPA mode).
  function init() {
    if (import.meta.client) {
      token.value = localStorage.getItem(TOKEN_KEY)
      username.value = localStorage.getItem(USER_KEY)
    }
  }

  async function login(user: string, password: string) {
    const res = await $fetch<LoginResponse>(`${config.public.apiBase}/auth/login`, {
      method: 'POST',
      body: { username: user, password }
    })
    token.value = res.token
    username.value = res.username
    if (import.meta.client) {
      localStorage.setItem(TOKEN_KEY, res.token)
      localStorage.setItem(USER_KEY, res.username)
    }
    return res
  }

  function logout() {
    token.value = null
    username.value = null
    if (import.meta.client) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(USER_KEY)
    }
  }

  const isLoggedIn = computed(() => !!token.value)

  return { token, username, isLoggedIn, init, login, logout }
}
