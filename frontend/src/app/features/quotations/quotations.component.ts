import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { ApiService } from '../../core/api.service';
import { QuotationListItem } from '../../core/models';

@Component({
  selector: 'app-quotations',
  imports: [RouterLink, DecimalPipe],
  template: `
    <div class="head">
      <h2>Quotations</h2>
      <a class="btn" routerLink="/quotations/new">+ New Quotation</a>
    </div>
    <table>
      <tr><th>Number</th><th>Customer</th><th>Status</th><th>Grand Total</th><th></th></tr>
      @for (q of quotations(); track q.id) {
        <tr>
          <td>{{ q.number }}</td><td>{{ q.customer }}</td>
          <td><span class="badge" [class]="q.status.toLowerCase()">{{ q.status }}</span></td>
          <td>{{ q.totals.grandTotal | number }}</td>
          <td class="row-actions">
            @if (q.status !== 'Accepted') {
              <button class="link" (click)="convert(q)">Convert to Invoice+Dispatch</button>
            } @else { <span class="muted">Converted</span> }
          </td>
        </tr>
      }
    </table>
    @if (message()) { <p class="msg">{{ message() }}</p> }
  `,
  styles: [`
    .head { display:flex; justify-content:space-between; align-items:center; margin-bottom:1rem; }
    .btn { background:#c2410c; color:#fff; padding:.5rem .9rem; border-radius:6px; text-decoration:none; }
    table { width:100%; background:#fff; border-collapse:collapse; border-radius:8px; overflow:hidden; }
    th, td { padding:.55rem .8rem; text-align:left; border-bottom:1px solid #e2e8f0; font-size:.88rem; }
    th { background:#f8fafc; }
    .link { background:none; border:none; color:#2563eb; cursor:pointer; padding:0; }
    .muted { color:#94a3b8; }
    .badge { padding:.15rem .55rem; border-radius:999px; font-size:.75rem; font-weight:600; background:#e2e8f0; }
    .badge.accepted { background:#dcfce7; color:#166534; }
    .badge.draft { background:#e2e8f0; color:#475569; }
    .badge.sent { background:#dbeafe; color:#1e40af; }
    .msg { margin-top:1rem; color:#166534; }
  `]
})
export class QuotationsComponent implements OnInit {
  private api = inject(ApiService);
  quotations = signal<QuotationListItem[]>([]);
  message = signal('');

  ngOnInit() { this.load(); }

  load() { this.api.getQuotations().subscribe((q) => this.quotations.set(q)); }

  convert(q: QuotationListItem) {
    this.api.convertQuotation(q.id).subscribe((res) => {
      this.message.set(`Created Invoice #${res.invoiceId} and Dispatch #${res.dispatchId}.`);
      this.load();
    });
  }
}
