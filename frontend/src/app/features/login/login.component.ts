import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  template: `
    <div class="login-wrap">
      <form class="login-card" (ngSubmit)="submit()">
        <h1>E&G Engineering</h1>
        <p class="sub">Inventory, Invoicing & Quotation System</p>
        <label>Username</label>
        <input [(ngModel)]="username" name="username" autocomplete="username" />
        <label>Password</label>
        <input type="password" [(ngModel)]="password" name="password" autocomplete="current-password" />
        @if (error()) { <p class="error">{{ error() }}</p> }
        <button type="submit" [disabled]="loading()">{{ loading() ? 'Signing in...' : 'Sign In' }}</button>
        <p class="hint">Demo login: admin / admin</p>
      </form>
    </div>
  `,
  styles: [`
    .login-wrap { display:flex; align-items:center; justify-content:center; min-height:100vh; background:#0f2b46; }
    .login-card { background:#fff; padding:2.5rem; border-radius:12px; width:340px; display:flex; flex-direction:column; box-shadow:0 10px 40px rgba(0,0,0,.3); }
    h1 { margin:0; color:#0f2b46; font-size:1.5rem; }
    .sub { color:#64748b; font-size:.85rem; margin:.25rem 0 1.5rem; }
    label { font-size:.8rem; font-weight:600; color:#334155; margin-top:.75rem; }
    input { padding:.6rem; border:1px solid #cbd5e1; border-radius:6px; margin-top:.25rem; }
    button { margin-top:1.5rem; padding:.7rem; background:#c2410c; color:#fff; border:none; border-radius:6px; font-weight:600; cursor:pointer; }
    button:disabled { opacity:.6; }
    .error { color:#dc2626; font-size:.8rem; margin-top:.75rem; }
    .hint { color:#94a3b8; font-size:.75rem; text-align:center; margin-top:1rem; }
  `]
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  username = 'admin';
  password = 'admin';
  loading = signal(false);
  error = signal('');

  submit() {
    this.loading.set(true);
    this.error.set('');
    this.auth.login(this.username, this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: () => {
        this.error.set('Invalid username or password');
        this.loading.set(false);
      }
    });
  }
}
