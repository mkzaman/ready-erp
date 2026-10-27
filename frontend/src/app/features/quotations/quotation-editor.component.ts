import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { Customer, DocHeader, Product, QuotationItem } from '../../core/models';

@Component({
  selector: 'app-quotation-editor',
  imports: [FormsModule],
  template: `
    <h2>New Quotation</h2>
    <div class="panel">
      <div class="grid">
        <label>Customer
          <select [(ngModel)]="model.customerId">
            <option [ngValue]="null">-- One-off (free text) --</option>
            @for (c of customers(); track c.id) { <option [ngValue]="c.id">{{ c.name }}</option> }
          </select>
        </label>
        <label>Or Customer Name<input [(ngModel)]="model.customerNameOverride" /></label>
        <label>Work Ref<input [(ngModel)]="model.workRef" /></label>
        <label>Project Site<input [(ngModel)]="model.projectSite" /></label>
        <label>PO No<input [(ngModel)]="model.poNo" /></label>
        <label>Job Ref<input [(ngModel)]="model.jobRef" /></label>
      </div>
    </div>

    <div class="panel">
      <div class="lines-head"><strong>Line Items</strong><button class="small" (click)="addLine()">+ Add Line</button></div>
      <table>
        <tr><th>Product</th><th>Description</th><th>Unit</th><th>Qty</th><th>Unit Price</th><th>Disc %</th><th></th></tr>
        @for (line of model.items; track $index) {
          <tr>
            <td>
              <select [(ngModel)]="line.productId" (ngModelChange)="onProduct(line)">
                <option [ngValue]="null">-- custom --</option>
                @for (p of products(); track p.id) { <option [ngValue]="p.id">{{ p.name }}</option> }
              </select>
            </td>
            <td><input [(ngModel)]="line.description" /></td>
            <td><input class="sm" [(ngModel)]="line.unit" /></td>
            <td><input class="sm" type="number" [(ngModel)]="line.quantity" /></td>
            <td><input type="number" [(ngModel)]="line.unitPrice" /></td>
            <td><input class="sm" type="number" [(ngModel)]="line.lineDiscountPercent" /></td>
            <td><button class="link" (click)="removeLine($index)">x</button></td>
          </tr>
        }
      </table>
    </div>

    <div class="panel charges">
      <label>Doc Discount %<input type="number" [(ngModel)]="model.documentDiscountPercent" /></label>
      <label>VAT %<input type="number" [(ngModel)]="model.vatPercent" /></label>
      <label>Service Charge<input type="number" [(ngModel)]="model.serviceCharge" /></label>
      <label>Weight Charge<input type="number" [(ngModel)]="model.weightCharge" /></label>
      <label>Delivery Charge<input type="number" [(ngModel)]="model.deliveryCharge" /></label>
    </div>

    <div class="actions">
      <button class="ghost" (click)="cancel()">Cancel</button>
      <button (click)="save()" [disabled]="model.items.length === 0">Save Quotation</button>
    </div>
  `,
  styles: [`
    .panel { background:#fff; padding:1.25rem; border-radius:8px; margin-bottom:1rem; }
    .grid { display:grid; grid-template-columns:1fr 1fr 1fr; gap:.75rem; }
    label { display:flex; flex-direction:column; font-size:.8rem; font-weight:600; color:#334155; }
    input, select { margin-top:.25rem; padding:.4rem; border:1px solid #cbd5e1; border-radius:6px; font-weight:400; }
    .lines-head { display:flex; justify-content:space-between; margin-bottom:.75rem; }
    table { width:100%; border-collapse:collapse; }
    th, td { padding:.3rem; text-align:left; font-size:.85rem; }
    td input, td select { margin-top:0; width:100%; }
    .sm { max-width:70px; }
    .charges { display:flex; gap:1rem; flex-wrap:wrap; }
    .charges label { flex:1; min-width:120px; }
    button { background:#c2410c; color:#fff; border:none; padding:.5rem .9rem; border-radius:6px; cursor:pointer; }
    button:disabled { opacity:.5; }
    .small { padding:.3rem .7rem; font-size:.8rem; }
    .link { background:none; color:#dc2626; padding:0; }
    .ghost { background:#e2e8f0; color:#334155; }
    .actions { display:flex; justify-content:flex-end; gap:.75rem; }
  `]
})
export class QuotationEditorComponent implements OnInit {
  private api = inject(ApiService);
  private router = inject(Router);

  customers = signal<Customer[]>([]);
  products = signal<Product[]>([]);

  model: DocHeader = {
    customerId: null, customerNameOverride: null, workRef: null, projectSite: null,
    poNo: null, poDate: null, jobRef: null, validUntil: null,
    documentDiscountPercent: 0, documentDiscountAmount: 0, vatPercent: 0,
    serviceCharge: 0, weightCharge: 0, deliveryCharge: 0, items: []
  };

  ngOnInit() {
    this.api.getCustomers().subscribe((c) => this.customers.set(c));
    this.api.getProducts().subscribe((p) => this.products.set(p));
  }

  addLine() {
    const line: QuotationItem = {
      productId: null, description: '', unit: 'Pcs', quantity: 1,
      unitPrice: 0, lineDiscountPercent: 0, lineDiscountAmount: 0, remarks: null
    };
    this.model.items.push(line);
  }
  removeLine(i: number) { this.model.items.splice(i, 1); }

  onProduct(line: QuotationItem) {
    const p = this.products().find((x) => x.id === line.productId);
    if (p) {
      line.description = p.name;
      line.unit = p.unit;
      line.unitPrice = p.sellingPrice;
    }
  }

  cancel() { this.router.navigate(['/quotations']); }

  save() {
    this.api.createQuotation(this.model).subscribe(() => this.router.navigate(['/quotations']));
  }
}
