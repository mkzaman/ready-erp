import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { Customer } from '../../core/models';

@Component({
  selector: 'app-customers',
  imports: [FormsModule],
  template: `
    <div class="head">
      <h2>Customers</h2>
      <button (click)="newCustomer()">+ New</button>
    </div>

    <table>
      <tr><th>Name</th><th>Contact</th><th>Phone</th><th>Email</th><th>Sites</th><th></th></tr>
      @for (c of customers(); track c.id) {
        <tr>
          <td>{{ c.name }}</td><td>{{ c.contactPerson }}</td><td>{{ c.phone }}</td>
          <td>{{ c.email }}</td><td>{{ c.sites.length }}</td>
          <td><button class="link" (click)="edit(c)">Edit</button></td>
        </tr>
      }
    </table>

    @if (editing(); as e) {
      <div class="modal-bg" (click)="cancel()">
        <div class="modal" (click)="$event.stopPropagation()">
          <h3>{{ e.id ? 'Edit' : 'New' }} Customer</h3>
          <div class="grid">
            <label>Name<input [(ngModel)]="e.name" /></label>
            <label>Contact Person<input [(ngModel)]="e.contactPerson" /></label>
            <label>Phone<input [(ngModel)]="e.phone" /></label>
            <label>Email<input [(ngModel)]="e.email" /></label>
            <label>VAT/BIN<input [(ngModel)]="e.vatBin" /></label>
            <label class="full">Billing Address<input [(ngModel)]="e.billingAddress" /></label>
          </div>
          <div class="sites">
            <div class="sites-head"><strong>Project Sites</strong><button class="small" (click)="addSite()">+ Add Site</button></div>
            @for (s of e.sites; track $index) {
              <div class="site-row">
                <input placeholder="Site name" [(ngModel)]="s.siteName" />
                <input placeholder="Address" [(ngModel)]="s.address" />
                <button class="small ghost" (click)="removeSite($index)">x</button>
              </div>
            }
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
    .head { display:flex; justify-content:space-between; align-items:center; margin-bottom:1rem; }
    table { width:100%; background:#fff; border-collapse:collapse; border-radius:8px; overflow:hidden; }
    th, td { padding:.55rem .8rem; text-align:left; border-bottom:1px solid #e2e8f0; font-size:.88rem; }
    th { background:#f8fafc; }
    button { background:#c2410c; color:#fff; border:none; padding:.45rem .9rem; border-radius:6px; cursor:pointer; }
    .link { background:none; color:#2563eb; padding:0; }
    .ghost { background:#e2e8f0; color:#334155; }
    .small { padding:.25rem .6rem; font-size:.8rem; }
    .modal-bg { position:fixed; inset:0; background:rgba(0,0,0,.4); display:flex; align-items:center; justify-content:center; }
    .modal { background:#fff; padding:1.5rem; border-radius:10px; width:640px; max-width:90%; max-height:90vh; overflow:auto; }
    .grid { display:grid; grid-template-columns:1fr 1fr; gap:.75rem; }
    .grid label { display:flex; flex-direction:column; font-size:.8rem; font-weight:600; color:#334155; }
    .grid label.full { grid-column:1 / -1; }
    .grid input { margin-top:.25rem; padding:.45rem; border:1px solid #cbd5e1; border-radius:6px; font-weight:400; }
    .sites { margin-top:1rem; border-top:1px solid #e2e8f0; padding-top:1rem; }
    .sites-head { display:flex; justify-content:space-between; align-items:center; margin-bottom:.5rem; }
    .site-row { display:flex; gap:.5rem; margin-bottom:.5rem; }
    .site-row input { flex:1; padding:.4rem; border:1px solid #cbd5e1; border-radius:6px; }
    .actions { display:flex; justify-content:flex-end; gap:.75rem; margin-top:1.25rem; }
  `]
})
export class CustomersComponent implements OnInit {
  private api = inject(ApiService);
  customers = signal<Customer[]>([]);
  editing = signal<Partial<Customer> | null>(null);

  ngOnInit() { this.load(); }

  load() { this.api.getCustomers().subscribe((c) => this.customers.set(c)); }

  newCustomer() { this.editing.set({ name: '', sites: [] }); }
  edit(c: Customer) { this.editing.set({ ...c, sites: c.sites.map((s) => ({ ...s })) }); }
  cancel() { this.editing.set(null); }

  addSite() { this.editing()!.sites!.push({ siteName: '', address: '' }); }
  removeSite(i: number) { this.editing()!.sites!.splice(i, 1); }

  save() {
    const e = this.editing()!;
    const op = e.id ? this.api.updateCustomer(e.id, e) : this.api.createCustomer(e);
    op.subscribe(() => { this.editing.set(null); this.load(); });
  }
}
