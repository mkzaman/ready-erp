import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { ShellComponent } from './features/shell/shell.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { ProductsComponent } from './features/products/products.component';
import { CustomersComponent } from './features/customers/customers.component';
import { QuotationsComponent } from './features/quotations/quotations.component';
import { QuotationEditorComponent } from './features/quotations/quotation-editor.component';
import { InvoicesComponent } from './features/invoices/invoices.component';
import { DispatchesComponent } from './features/dispatches/dispatches.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'products', component: ProductsComponent },
      { path: 'customers', component: CustomersComponent },
      { path: 'quotations', component: QuotationsComponent },
      { path: 'quotations/new', component: QuotationEditorComponent },
      { path: 'invoices', component: InvoicesComponent },
      { path: 'dispatches', component: DispatchesComponent },
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
