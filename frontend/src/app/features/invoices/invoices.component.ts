import { Component, inject, OnInit, signal } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-invoices',
  imports: [DecimalPipe, DatePipe],
  template: `
    <h2>Invoices</h2>
    <table>
      <tr><th>Number</th><th>Customer</th><th>Grand Total</th><th>Status</th><th></th></tr>
      @for (i of invoices(); track i.id) {
        <tr>
          <td>{{ i.number }}</td><td>{{ i.customer }}</td>
          <td>{{ i.totals.grandTotal | number }}</td>
          <td>{{ i.isVoided ? 'Voided' : 'Active' }}</td>
          <td><button class="link" (click)="view(i.id)">View / Print</button></td>
        </tr>
      }
    </table>

    @if (current(); as doc) {
      <div class="modal-bg" (click)="current.set(null)">
        <div class="print-area" (click)="$event.stopPropagation()">
          <div class="no-print bar">
            <button (click)="print()">Print</button>
            <button class="ghost" (click)="current.set(null)">Close</button>
          </div>
          <div class="doc">
            <div class="doc-head">
              <div><h1>E&G Engineering Ltd.</h1><p>Inventory-Based Invoicing System</p></div>
              <div class="doc-title"><h2>INVOICE</h2><p>{{ doc.invoice.number }}</p><p>{{ doc.invoice.date | date:'dd.MM.yyyy' }}</p></div>
            </div>
            <div class="parties">
              <div>
                <strong>{{ doc.invoice.customer?.name || doc.invoice.customerNameOverride }}</strong>
                <p>{{ doc.invoice.customer?.billingAddress }}</p>
                <p>{{ doc.invoice.customer?.contactPerson }} {{ doc.invoice.customer?.phone }}</p>
              </div>
              <div class="job">
                <p><b>Work Ref:</b> {{ doc.invoice.workRef }}</p>
                <p><b>Project Site:</b> {{ doc.invoice.projectSite }}</p>
                <p><b>PO No:</b> {{ doc.invoice.poNo }}</p>
                <p><b>Job Ref:</b> {{ doc.invoice.jobRef }}</p>
              </div>
            </div>
            <table class="items">
              <tr><th>SL</th><th>Description</th><th>UoM</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>
              @for (it of doc.invoice.items; track it.id; let idx = $index) {
                <tr>
                  <td>{{ idx + 1 }}</td><td>{{ it.description }}</td><td>{{ it.unit }}</td>
                  <td>{{ it.quantity }}</td><td>{{ it.unitPrice | number }}</td>
                  <td>{{ (it.quantity * it.unitPrice) | number }}</td>
                </tr>
              }
            </table>
            <div class="totals">
              <p><span>Sub-Total</span><span>{{ doc.totals.subTotal | number }}</span></p>
              @if (doc.totals.charges) { <p><span>Charges</span><span>{{ doc.totals.charges | number }}</span></p> }
              @if (doc.invoice.vatPercent) { <p><span>Add VAT ({{ doc.invoice.vatPercent }}%)</span><span>{{ doc.totals.vat | number }}</span></p> }
              <p class="grand"><span>Grand Total</span><span>{{ doc.totals.grandTotal | number }}</span></p>
              <p class="words">In Word: {{ doc.totals.grandTotalInWords }}</p>
            </div>
            <div class="signs"><span>Prepared By</span><span>Received By</span></div>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    table { width:100%; background:#fff; border-collapse:collapse; border-radius:8px; overflow:hidden; }
    th, td { padding:.55rem .8rem; text-align:left; border-bottom:1px solid #e2e8f0; font-size:.88rem; }
    th { background:#f8fafc; }
    .link { background:none; border:none; color:#2563eb; cursor:pointer; padding:0; }
    .modal-bg { position:fixed; inset:0; background:rgba(0,0,0,.4); display:flex; align-items:flex-start; justify-content:center; overflow:auto; padding:2rem; }
    .print-area { background:#fff; border-radius:8px; width:800px; }
    .bar { display:flex; justify-content:flex-end; gap:.75rem; padding:1rem; }
    .bar button { background:#c2410c; color:#fff; border:none; padding:.5rem .9rem; border-radius:6px; cursor:pointer; }
    .bar .ghost { background:#e2e8f0; color:#334155; }
    .doc { padding:2rem; }
    .doc-head { display:flex; justify-content:space-between; border-bottom:2px solid #0f2b46; padding-bottom:1rem; }
    .doc-head h1 { margin:0; color:#0f2b46; font-size:1.4rem; }
    .doc-title { text-align:right; }
    .doc-title h2 { margin:0; color:#c2410c; }
    .parties { display:flex; justify-content:space-between; margin:1.5rem 0; font-size:.85rem; }
    .parties p { margin:.1rem 0; }
    .items { border:1px solid #e2e8f0; }
    .items th { background:#0f2b46; color:#fff; }
    .totals { margin-top:1rem; margin-left:auto; width:300px; font-size:.9rem; }
    .totals p { display:flex; justify-content:space-between; margin:.25rem 0; }
    .totals .grand { border-top:2px solid #0f2b46; padding-top:.5rem; font-weight:700; }
    .totals .words { font-style:italic; font-size:.8rem; color:#475569; }
    .signs { display:flex; justify-content:space-between; margin-top:3rem; }
    .signs span { border-top:1px solid #334155; padding-top:.5rem; width:180px; text-align:center; font-size:.85rem; }
    @media print {
      .no-print { display:none; }
      .modal-bg { position:static; background:none; padding:0; }
      .print-area { width:100%; }
    }
  `]
})
export class InvoicesComponent implements OnInit {
  private api = inject(ApiService);
  invoices = signal<any[]>([]);
  current = signal<any | null>(null);

  ngOnInit() { this.api.getInvoices().subscribe((i) => this.invoices.set(i)); }

  view(id: number) { this.api.getInvoice(id).subscribe((d) => this.current.set(d)); }
  print() { window.print(); }
}
