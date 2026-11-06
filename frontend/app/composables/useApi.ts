import type {
  Category,
  Customer,
  DashboardData,
  DocHeader,
  Location,
  Paged,
  Product,
  ProductQuery,
  ProductUpsert,
  QuotationListItem,
  Warehouse
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

  /** Drops null/undefined/empty values so they are not sent as "null". */
  function qs(params: Record<string, unknown>) {
    const search = new URLSearchParams()
    for (const [key, value] of Object.entries(params)) {
      if (value === null || value === undefined || value === '') continue
      search.set(key, String(value))
    }
    const str = search.toString()
    return str ? `?${str}` : ''
  }

  return {
    // Products
    getProducts: (query: ProductQuery = {}) =>
      request<Paged<Product>>(`/products${qs(query as Record<string, unknown>)}`),
    getProduct: (id: number) => request<Product>(`/products/${id}`),
    createProduct: (p: ProductUpsert) =>
      request<Product>('/products', { method: 'POST', body: p }),
    updateProduct: (id: number, p: ProductUpsert) =>
      request<Product>(`/products/${id}`, { method: 'PUT', body: p }),
    deactivateProduct: (id: number) =>
      request(`/products/${id}/deactivate`, { method: 'POST' }),
    activateProduct: (id: number) =>
      request(`/products/${id}/activate`, { method: 'POST' }),
    adjustStock: (id: number, quantityChange: number, reason: string) =>
      request<Product>(`/products/${id}/adjust-stock`, {
        method: 'POST',
        body: { quantityChange, reason }
      }),
    uploadProductImage: (id: number, file: File) => {
      // Let the browser set the multipart boundary; do not set Content-Type.
      const form = new FormData()
      form.append('file', file)
      return request<Product>(`/products/${id}/image`, { method: 'POST', body: form })
    },
    deleteProductImage: (id: number) =>
      request(`/products/${id}/image`, { method: 'DELETE' }),

    // Reference data
    getCategories: () => request<Category[]>('/categories'),
    createCategory: (name: string, description: string | null = null) =>
      request<Category>('/categories', { method: 'POST', body: { name, description } }),
    getWarehouses: () => request<Warehouse[]>('/warehouses'),
    getWarehouseLocations: (warehouseId: number) =>
      request<Location[]>(`/warehouses/${warehouseId}/locations`),
    getLocations: (warehouseId?: number) =>
      request<Location[]>(`/locations${qs({ warehouseId })}`),

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
