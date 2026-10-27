import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { Product } from '../../core/models';

@Component({
  selector: 'app-products',
  imports: [FormsModule, DecimalPipe],
  template: `
    <div class="head">
      <h2>Products & Services</h2>
      <button (click)="newProduct()">+ New</button>
    </div>
    <input class="search" placeholder="Search by part no, name, category..." [(ngModel)]="search" (ngModelChange)="load()" />

    <table>
      <tr><th>Part No</th><th>Name</th><th>Category</th><th>Type</th><th>Unit</th><th>Selling</th><th>Stock</th><th></th></tr>
      @for (p of products(); track p.id) {
        <tr>
          <td>{{ p.partNumber }}</td><td>{{ p.name }}</td><td>{{ p.category }}</td>
          <td>{{ p.type }}</td><td>{{ p.unit }}</td><td>{{ p.sellingPrice | number }}</td>
          <td [class.warn]="p.lowStockThreshold != null && p.stockQuantity <= p.lowStockThreshold">
            {{ p.type === 'Service' ? '-' : p.stockQuantity }}
          </td>
          <td><button class="link" (click)="edit(p)">Edit</button></td>
        </tr>
      }
    </table>

    @if (editing(); as e) {
      <div class="modal-bg" (click)="cancel()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>{{ e.id ? 'Edit' : 'New' }} Product</h3>
          <div class="grid">
            <label>Part Number<input [(ngModel)]="e.partNumber" /></label>
            <label>Name<input [(ngModel)]="e.name" /></label>
            <label>Category<input [(ngModel)]="e.category" /></label>
            <label>Unit<input [(ngModel)]="e.unit" /></label>
            <label>Type
              <select [(ngModel)]="e.type"><option>Product</option><option>Service</option></select>
            </label>
            <label>Buying Price<input type="number" [(ngModel)]="e.buyingPrice" /></label>
            <label>Selling Price<input type="number" [(ngModel)]="e.sellingPrice" /></label>
            <label>Stock Qty<input type="number" [(ngModel)]="e.stockQuantity" /></label>
            <label>Low Stock Threshold<input type="number" [(ngModel)]="e.lowStockThreshold" /></label>
          </div>
          <div class="actions">
            <button class="ghost" (click)="cancel()">Cancel</button>
            <button (click)="save()">Save</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .head { display:flex; justify-content:space-between; align-items:center; }
    .search { width:100%; padding:.6rem; margin:1rem 0; border:1px solid #cbd5e1; border-radius:6px; }
    table { width:100%; background:#fff; border-collapse:collapse; border-radius:8px; overflow:hidden; }
    th, td { padding:.55rem .8rem; text-align:left; border-bottom:1px solid #e2e8f0; font-size:.88rem; }
    th { background:#f8fafc; }
    .warn { color:#dc2626; font-weight:600; }
    button { background:#c2410c; color:#fff; border:none; padding:.45rem .9rem; border-radius:6px; cursor:pointer; }
    .link { background:none; color:#2563eb; padding:0; }
    .ghost { background:#e2e8f0; color:#334155; }
    .modal-bg { position:fixed; inset:0; background:rgba(0,0,0,.4); display:flex; align-items:center; justify-content:center; }
    .modal { background:#fff; padding:1.5rem; border-radius:10px; width:640px; max-width:90%; }
    .grid { display:grid; grid-template-columns:1fr 1fr; gap:.75rem; }
    .grid label { display:flex; flex-direction:column; font-size:.8rem; font-weight:600; color:#334155; }
    .grid input, .grid select { margin-top:.25rem; padding:.45rem; border:1px solid #cbd5e1; border-radius:6px; font-weight:400; }
    .actions { display:flex; justify-content:flex-end; gap:.75rem; margin-top:1.25rem; }
  `]
})
export class ProductsComponent implements OnInit {
  private api = inject(ApiService);
  products = signal<Product[]>([]);
  editing = signal<Partial<Product> | null>(null);
  search = '';

  ngOnInit() { this.load(); }

  load() {
    this.api.getProducts(this.search).subscribe((p) => this.products.set(p));
  }

  newProduct() {
    this.editing.set({ type: 'Product', unit: 'Pcs', buyingPrice: 0, sellingPrice: 0, stockQuantity: 0, lowStockThreshold: null });
  }
  edit(p: Product) { this.editing.set({ ...p }); }
  cancel() { this.editing.set(null); }

  save() {
    const e = this.editing()!;
    const op = e.id ? this.api.updateProduct(e.id, e) : this.api.createProduct(e);
    op.subscribe(() => { this.editing.set(null); this.load(); });
  }
}
