import { Component, inject, OnInit, signal } from '@angular/core';
import { ApiService } from '../../core/api.service';
import { DashboardData } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  template: `
    <h2>Dashboard</h2>
    @if (data(); as d) {
      <div class="cards">
        <div class="card"><span>{{ d.productCount }}</span>Products</div>
        <div class="card"><span>{{ d.customerCount }}</span>Customers</div>
        <div class="card"><span>{{ d.quotationCount }}</span>Quotations</div>
        <div class="card"><span>{{ d.invoiceCount }}</span>Invoices</div>
        <div class="card"><span>{{ d.dispatchCount }}</span>Dispatches</div>
      </div>
      <h3>Low Stock</h3>
      @if (d.lowStock.length === 0) { <p class="muted">No low-stock items.</p> }
      @else {
        <table>
          <tr><th>Part No</th><th>Name</th><th>Stock</th><th>Threshold</th></tr>
          @for (item of d.lowStock; track item.partNumber) {
            <tr><td>{{ item.partNumber }}</td><td>{{ item.name }}</td><td class="warn">{{ item.stockQuantity }}</td><td>{{ item.lowStockThreshold }}</td></tr>
          }
        </table>
      }
    }
  `,
  styles: [`
    .cards { display:flex; gap:1rem; flex-wrap:wrap; margin-bottom:2rem; }
    .card { background:#fff; border-radius:10px; padding:1.5rem; min-width:130px; display:flex; flex-direction:column; box-shadow:0 1px 3px rgba(0,0,0,.08); color:#64748b; font-size:.85rem; }
    .card span { font-size:2rem; font-weight:700; color:#0f2b46; }
    table { width:100%; background:#fff; border-collapse:collapse; border-radius:8px; overflow:hidden; }
    th, td { padding:.6rem .9rem; text-align:left; border-bottom:1px solid #e2e8f0; font-size:.9rem; }
    th { background:#f8fafc; }
    .warn { color:#dc2626; font-weight:600; }
    .muted { color:#94a3b8; }
  `]
})
export class DashboardComponent implements OnInit {
  private api = inject(ApiService);
  data = signal<DashboardData | null>(null);

  ngOnInit() {
    this.api.getDashboard().subscribe((d) => this.data.set(d));
  }
}
