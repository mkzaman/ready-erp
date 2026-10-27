import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="layout">
      <aside>
        <div class="brand">E&G ERP</div>
        <nav>
          <a routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a routerLink="/products" routerLinkActive="active">Products</a>
          <a routerLink="/customers" routerLinkActive="active">Customers</a>
          <a routerLink="/quotations" routerLinkActive="active">Quotations</a>
          <a routerLink="/invoices" routerLinkActive="active">Invoices</a>
          <a routerLink="/dispatches" routerLinkActive="active">Dispatches</a>
        </nav>
      </aside>
      <main>
        <header>
          <span>{{ auth.username() }}</span>
          <button (click)="logout()">Logout</button>
        </header>
        <div class="content"><router-outlet /></div>
      </main>
    </div>
  `,
  styles: [`
    .layout { display:flex; min-height:100vh; }
    aside { width:220px; background:#0f2b46; color:#cbd5e1; display:flex; flex-direction:column; }
    .brand { padding:1.25rem; font-weight:700; font-size:1.2rem; color:#fff; border-bottom:1px solid #1e3a5f; }
    nav { display:flex; flex-direction:column; padding:.5rem; }
    nav a { padding:.7rem 1rem; color:#cbd5e1; text-decoration:none; border-radius:6px; font-size:.9rem; }
    nav a.active, nav a:hover { background:#1e3a5f; color:#fff; }
    main { flex:1; background:#f1f5f9; display:flex; flex-direction:column; }
    header { display:flex; justify-content:flex-end; align-items:center; gap:1rem; padding:.75rem 1.5rem; background:#fff; border-bottom:1px solid #e2e8f0; }
    header button { background:#c2410c; color:#fff; border:none; padding:.4rem .9rem; border-radius:6px; cursor:pointer; }
    .content { padding:1.5rem; }
  `]
})
export class ShellComponent {
  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
