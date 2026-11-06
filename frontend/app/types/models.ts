/** Envelope returned by server-side paged list endpoints. */
export interface Paged<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

/** Derived by the API so the stock rule lives in exactly one place. */
export type StockState = 'InStock' | 'LowStock' | 'OutOfStock' | 'NotTracked'

export interface Product {
  id: number
  partNumber: string
  name: string
  unit: string
  type: string
  buyingPrice: number
  sellingPrice: number
  stockQuantity: number
  lowStockThreshold: number | null
  isActive: boolean
  categoryId: number | null
  categoryName: string | null
  warehouseId: number | null
  warehouseName: string | null
  locationId: number | null
  locationCode: string | null
  /** Absolute URL, or null when no image has been uploaded. */
  imagePath: string | null
  stockState: StockState
}

/** Request body for create/update. Note it sends ids, not names. */
export interface ProductUpsert {
  partNumber: string
  name: string
  unit: string
  type: string
  buyingPrice: number
  sellingPrice: number
  stockQuantity: number
  lowStockThreshold: number | null
  categoryId: number | null
  warehouseId: number | null
  locationId: number | null
}

export interface Category {
  id: number
  name: string
  description: string | null
  isActive: boolean
  productCount: number
}

export interface Warehouse {
  id: number
  code: string
  name: string
  address: string | null
  isActive: boolean
  locationCount: number
}

export interface Location {
  id: number
  warehouseId: number
  warehouseName: string
  code: string
  description: string | null
  isActive: boolean
}

/** Query parameters accepted by GET /products. */
export interface ProductQuery {
  search?: string
  categoryId?: number | null
  warehouseId?: number | null
  locationId?: number | null
  status?: StockState | ''
  includeInactive?: boolean
  page?: number
  pageSize?: number
}

export interface CustomerSite {
  id?: number
  siteName: string
  address: string | null
}

export interface Customer {
  id: number
  name: string
  billingAddress: string | null
  contactPerson: string | null
  phone: string | null
  email: string | null
  vatBin: string | null
  isActive: boolean
  sites: CustomerSite[]
}

export interface Totals {
  subTotal: number
  discount: number
  charges: number
  vat: number
  grandTotal: number
  grandTotalInWords: string
}

export interface QuotationItem {
  productId: number | null
  description: string
  unit: string
  quantity: number
  unitPrice: number
  lineDiscountPercent: number
  lineDiscountAmount: number
  remarks: string | null
}

export interface QuotationListItem {
  id: number
  number: string
  date: string
  status: string
  customer: string | null
  totals: Totals
}

export interface DocHeader {
  customerId: number | null
  customerNameOverride: string | null
  workRef: string | null
  projectSite: string | null
  poNo: string | null
  poDate: string | null
  jobRef: string | null
  validUntil?: string | null
  documentDiscountPercent: number
  documentDiscountAmount: number
  vatPercent: number
  serviceCharge: number
  weightCharge: number
  deliveryCharge: number
  items: QuotationItem[]
}

export interface ReorderItem {
  id: number
  partNumber: string
  name: string
  stockQuantity: number
  lowStockThreshold: number | null
  categoryName: string | null
  stockState: StockState
}

export interface DashboardData {
  productCount: number
  customerCount: number
  quotationCount: number
  invoiceCount: number
  dispatchCount: number
  lowStockCount: number
  outOfStockCount: number
  totalStockValue: number
  unitsDispatchedToday: number
  dispatchesPendingIssue: number
  /** At or below reorder point, worst first. Includes out-of-stock rows. */
  reorderList: ReorderItem[]
  /** Units dispatched per day for the last 7 days, oldest first. */
  dispatchTrend: number[]
}

export interface LoginResponse {
  token: string
  username: string
  role: string
}
