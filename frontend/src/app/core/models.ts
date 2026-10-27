export interface Product {
  id: number;
  partNumber: string;
  name: string;
  category: string;
  unit: string;
  type: string;
  buyingPrice: number;
  sellingPrice: number;
  stockQuantity: number;
  lowStockThreshold: number | null;
  isActive: boolean;
}

export interface CustomerSite {
  id?: number;
  siteName: string;
  address: string | null;
}

export interface Customer {
  id: number;
  name: string;
  billingAddress: string | null;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  vatBin: string | null;
  isActive: boolean;
  sites: CustomerSite[];
}

export interface Totals {
  subTotal: number;
  discount: number;
  charges: number;
  vat: number;
  grandTotal: number;
  grandTotalInWords: string;
}

export interface QuotationItem {
  productId: number | null;
  description: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  lineDiscountPercent: number;
  lineDiscountAmount: number;
  remarks: string | null;
}

export interface QuotationListItem {
  id: number;
  number: string;
  date: string;
  status: string;
  customer: string | null;
  totals: Totals;
}

export interface DocHeader {
  customerId: number | null;
  customerNameOverride: string | null;
  workRef: string | null;
  projectSite: string | null;
  poNo: string | null;
  poDate: string | null;
  jobRef: string | null;
  validUntil?: string | null;
  documentDiscountPercent: number;
  documentDiscountAmount: number;
  vatPercent: number;
  serviceCharge: number;
  weightCharge: number;
  deliveryCharge: number;
  items: QuotationItem[];
}

export interface DashboardData {
  productCount: number;
  customerCount: number;
  quotationCount: number;
  invoiceCount: number;
  dispatchCount: number;
  lowStock: { name: string; partNumber: string; stockQuantity: number; lowStockThreshold: number }[];
}
