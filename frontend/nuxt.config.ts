import tailwindcss from '@tailwindcss/vite'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },
  ssr: false,
  modules: ['@nuxt/fonts', '@nuxt/icon'],
  css: ['~/assets/css/main.css'],
  vite: {
    plugins: [tailwindcss()]
  },
  fonts: {
    families: [
      { name: 'Inter', provider: 'google', weights: [400, 500, 600] },
      { name: 'Plus Jakarta Sans', provider: 'google', weights: [600, 700] }
    ]
  },
  icon: {
    // Bundle Material Symbols locally so there is no runtime API call.
    clientBundle: {
      scan: true,
      sizeLimitKb: 512
    }
  },
  runtimeConfig: {
    public: {
      apiBase: 'https://ready-erp.onrender.com/api'
    }
  },
  app: {
    head: {
      title: 'E&G Engineering ERP',
      meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }]
    }
  }
})
