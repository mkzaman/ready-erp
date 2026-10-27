import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from './environment';
import { Customer, DashboardData, DocHeader, Product, QuotationListItem } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  // Products
  getProducts(search = '') {
    return this.http.get<Product[]>(`${this.base}/products`, { params: { search } });
  }
  createProduct(p: Partial<Product>) {
    return this.http.post<Product>(`${this.base}/products`, p);
  }
  updateProduct(id: number, p: Partial<Product>) {
    return this.http.put<Product>(`${this.base}/products/${id}`, p);
  }
  deactivateProduct(id: number) {
    return this.http.post(`${this.base}/products/${id}/deactivate`, {});
  }
  adjustStock(id: number, quantityChange: number, reason: string) {
    return this.http.post<Product>(`${this.base}/products/${id}/adjust-stock`, { quantityChange, reason });
  }

  // Customers
  getCustomers(search = '') {
    return this.http.get<Customer[]>(`${this.base}/customers`, { params: { search } });
  }
  createCustomer(c: Partial<Customer>) {
    return this.http.post<Customer>(`${this.base}/customers`, c);
  }
  updateCustomer(id: number, c: Partial<Customer>) {
    return this.http.put<Customer>(`${this.base}/customers/${id}`, c);
  }

  // Quotations
  getQuotations() {
    return this.http.get<QuotationListItem[]>(`${this.base}/quotations`);
  }
  getQuotation(id: number) {
    return this.http.get<any>(`${this.base}/quotations/${id}`);
  }
  createQuotation(q: DocHeader) {
    return this.http.post<{ id: number }>(`${this.base}/quotations`, q);
  }
  setQuotationStatus(id: number, status: string) {
    return this.http.post(`${this.base}/quotations/${id}/status/${status}`, {});
  }
  convertQuotation(id: number) {
    return this.http.post<{ invoiceId: number; dispatchId: number }>(`${this.base}/quotations/${id}/convert`, {});
  }

  // Invoices
  getInvoices() {
    return this.http.get<any[]>(`${this.base}/invoices`);
  }
  getInvoice(id: number) {
    return this.http.get<any>(`${this.base}/invoices/${id}`);
  }

  // Dispatches
  getDispatches() {
    return this.http.get<any[]>(`${this.base}/dispatches`);
  }
  getDispatch(id: number) {
    return this.http.get<any>(`${this.base}/dispatches/${id}`);
  }
  issueDispatch(id: number) {
    return this.http.post<any>(`${this.base}/dispatches/${id}/issue`, {});
  }

  // Dashboard
  getDashboard() {
    return this.http.get<DashboardData>(`${this.base}/dashboard`);
  }
}
