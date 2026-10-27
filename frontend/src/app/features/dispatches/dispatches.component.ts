import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ApiService } from '../../core/api.service';

@Component({
  selector: 'app-dispatches',
  imports: [DatePipe],
  template: `
    <h2>Dispatch Notes</h2>
    <table>
      <tr><th>Number</th><th>Customer</th><th>Lines</th><th>Status</th><th></th></tr>
      @for (d of dispatches(); track d.id) {
        <tr>
          <td>{{ d.number }}</td><td>{{ d.customer }}</td><td>{{ d.lineCount }}</td>
          <td>{{ d.isIssued ? 'Issued' : 'Pending' }}</td>
          <td class="row-actions">
            <button class="link" (click)="view(d.id)">View / Print</button>
            @if (!d.isIssued) { <button class="link orange" (click)="issue(d.id)">Issue (reduce stock)</button> }
          </td>
        </tr>
      }
    </table>
    @if (message()) { <p class="msg">{{ message() }}</p> }

    @if (current(); as doc) {
      <div class="modal-bg" (click)="current.set(null)">
        <div class="print-area" (click)="$event.stopPropagation()">
          <div class="no-print bar">
            <button (click)="print()">Print</button>
            <button class="ghost" (click)="current.set(null)">Close</button>
          </div>
          <div class="doc">
            <div class="doc-head">
              <div><h1>E&G Engineering Ltd.</h1></div>
              <div class="doc-title"><h2>DISPATCH NOTE</h2><p>{{ doc.dispatch.number }}</p><p>{{ doc.dispatch.date | date:'dd.MM.yyyy' }}</p></div>
            </div>
            <div class="parties">
              <div>
                <strong>{{ doc.dispatch.customer?.name || doc.dispatch.customerNameOverride }}</strong>
                <p>{{ doc.dispatch.customer?.billingAddress }}</p>
              </div>
              <div class="job">
                <p><b>Work Ref:</b> {{ doc.dispatch.workRef }}</p>
                <p><b>Project Site:</b> {{ doc.dispatch.projectSite }}</p>
                <p><b>PO No:</b> {{ doc.dispatch.poNo }}</p>
              </div>
            </div>
            <table class="items">
              <tr><th>SL</th><th>Description</th><th>UoM</th><th>Qty</th></tr>
              @for (it of doc.dispatch.items; track it.id; let idx = $index) {
                <tr><td>{{ idx + 1 }}</td><td>{{ it.description }}</td><td>{{ it.unit }}</td><td>{{ it.quantity }}</td></tr>
              }
            </table>
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
    .link { background:none; border:none; color:#2563eb; cursor:pointer; padding:0; margin-right:1rem; }
    .link.orange { color:#c2410c; }
    .msg { margin-top:1rem; color:#166534; }
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
    .items { width:100%; border:1px solid #e2e8f0; }
    .items th { background:#0f2b46; color:#fff; }
    .signs { display:flex; justify-content:space-between; margin-top:3rem; }
    .signs span { border-top:1px solid #334155; padding-top:.5rem; width:180px; text-align:center; font-size:.85rem; }
    @media print {
      .no-print { display:none; }
      .modal-bg { position:static; background:none; padding:0; }
      .print-area { width:100%; }
    }
  `]
})
export class DispatchesComponent implements OnInit {
  private api = inject(ApiService);
  dispatches = signal<any[]>([]);
  current = signal<any | null>(null);
  message = signal('');

  ngOnInit() { this.load(); }

  load() { this.api.getDispatches().subscribe((d) => this.dispatches.set(d)); }

  view(id: number) { this.api.getDispatch(id).subscribe((d) => this.current.set(d)); }

  issue(id: number) {
    this.api.issueDispatch(id).subscribe((res) => {
      this.message.set(res.warnings?.length
        ? `Issued with stock warnings: ${res.warnings.join('; ')}`
        : 'Dispatch issued. Stock reduced.');
      this.load();
    });
  }

  print() { window.print(); }
}
