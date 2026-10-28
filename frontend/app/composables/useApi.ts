import type {
  Customer,
  DashboardData,
  DocHeader,
  Product,
  QuotationListItem
} from '~/types/models'

export function useApi() {
  const config = useRuntimeConfig()
  const { token, logout } = useAuth()

  function request<T>(path: string, opts: Record<string, any> = {}): Promise<T> {
    return $fetch<T>(`${config.public.apiBase}${path}`, {
      ...opts,
      headers: {
        ...(opts.headers || {}),
        ...(token.value ? { Authorization: `Bearer ${token.value}` } : {})
      },
      onResponseError({ response }) {
        if (response.status === 401) {
          logout()
          navigateTo('/login')
        }
      }
    })
  }

  return {
    // Products
    getProducts: (search = '') =>
      request<Product[]>(`/products?search=${encodeURIComponent(search)}`),
    createProduct: (p: Partial<Product>) =>
      request<Product>('/products', { method: 'POST', body: p }),
    updateProduct: (id: number, p: Partial<Product>) =>
      request<Product>(`/products/${id}`, { method: 'PUT', body: p }),
    deactivateProduct: (id: number) =>
      request(`/products/${id}/deactivate`, { method: 'POST' }),
    adjustStock: (id: number, quantityChange: number, reason: string) =>
      request<Product>(`/products/${id}/adjust-stock`, {
        method: 'POST',
        body: { quantityChange, reason }
      }),

    // Customers
    getCustomers: (search = '') =>
      request<Customer[]>(`/customers?search=${encodeURIComponent(search)}`),
    createCustomer: (c: Partial<Customer>) =>
      request<Customer>('/customers', { method: 'POST', body: c }),
    updateCustomer: (id: number, c: Partial<Customer>) =>
      request<Customer>(`/customers/${id}`, { method: 'PUT', body: c }),

    // Quotations
    getQuotations: () => request<QuotationListItem[]>('/quotations'),
    getQuotation: (id: number) => request<any>(`/quotations/${id}`),
    createQuotation: (q: DocHeader) =>
      request<{ id: number }>('/quotations', { method: 'POST', body: q }),
    setQuotationStatus: (id: number, status: string) =>
      request(`/quotations/${id}/status/${status}`, { method: 'POST' }),
    convertQuotation: (id: number) =>
      request<{ invoiceId: number; dispatchId: number }>(`/quotations/${id}/convert`, {
        method: 'POST'
      }),

    // Invoices
    getInvoices: () => request<any[]>('/invoices'),
    getInvoice: (id: number) => request<any>(`/invoices/${id}`),

    // Dispatches
    getDispatches: () => request<any[]>('/dispatches'),
    getDispatch: (id: number) => request<any>(`/dispatches/${id}`),
    issueDispatch: (id: number) =>
      request<any>(`/dispatches/${id}/issue`, { method: 'POST' }),

    // Dashboard
    getDashboard: () => request<DashboardData>('/dashboard')
  }
}
