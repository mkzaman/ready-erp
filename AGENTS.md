# AGENTS.md

Client-demo MVP for E&G Engineering: inventory, quotations, invoices, dispatch. Two independent apps — `backend/` (ASP.NET Core + EF Core + SQLite) and `frontend/` (Nuxt 3, SPA mode). Not a monorepo; no root build. The source requirement is `E&G_Engineering_Proposal.pdf`; `IMPLEMENTATION_CHECKLIST.md` tracks scope.

## Run (needs two terminals)
```powershell
cd backend ; dotnet run --urls http://localhost:5080   # API
cd frontend ; npm run dev                               # http://localhost:3000
```
- **Node 20+ is required** (Nuxt 4). Node 16 fails with `node:util has no export parseArgs`. Use `nvm use 22.23.1`.
- Login: **admin / admin** (seeded). No registration, no email flows.
- Frontend expects the API at `http://localhost:5080/api` — set via `runtimeConfig.public.apiBase` in `frontend/nuxt.config.ts`. CORS in `backend/Program.cs` only allows `http://localhost:3000`. Change both together if a port moves.

## Backend quirks
- **No EF migrations.** Schema is created via `DbContext.Database.EnsureCreated()` in `Infrastructure/DbSeeder.cs`, which also seeds admin + sample data. To change the schema, delete `backend/readyerp.db` (plus `-shm` / `-wal`, all gitignored) and re-run; do not scaffold migrations unless you intend to switch the whole approach.
- JSON uses `ReferenceHandler.IgnoreCycles` (set in `Program.cs`) because controllers return EF entities with navigation cycles (e.g. `Customer` ↔ `Sites`). Keep it, or return DTOs instead.
- Passwords are SHA-256 (`Infrastructure/PasswordHasher.cs`) and the JWT secret is hardcoded in `Services/TokenService.cs` — demo-only, not production auth.
- Structure is layered by folder: `Domain/`, `Infrastructure/`, `Services/`, `Dtos/`, `Controllers/`.
- **Uploads**: product images are written to `Storage:UploadPath` (`appsettings.json`, default `wwwroot/uploads`) and served anonymously at `/uploads` via `PhysicalFileProvider`. `ProductDto.ImagePath` is stored relative but returned absolute, because the SPA is on a different origin. To containerise, point `Storage:UploadPath` **and** the SQLite file at one mounted volume.
- Business rules that are easy to break:
  - Document numbers share ONE running sequence across quotations/invoices/dispatches (`Services/NumberingService.cs`), format `EGEL/<Type>/<Year>/<No>`.
  - Stock only decrements when a dispatch is **issued** (`DispatchesController.Issue`), never on quote/invoice; services (`ItemType.Service`) are skipped. Short stock = warn-and-allow, not block (stock may go negative).
  - `Product.StockQuantity` is a **single** figure. `Warehouse` / `Location` are metadata describing where stock normally sits — there is no per-warehouse balance, and `Issue` must stay unaware of them.
  - Stock state has exactly one definition, shared by `ProductsController` (`?status=`) and `DashboardController`: `OutOfStock` = qty ≤ 0, `LowStock` = qty > 0 && qty ≤ threshold, `NotTracked` = services. Keep both in sync or the KPI and the filter will disagree.
  - Totals/VAT/amount-in-words live in `Services/PricingCalculator.cs` and `AmountInWords.cs`; charges are added before VAT. `frontend/app/utils/pricing.ts` mirrors this for the live preview — **change both together**.
  - `GET /api/products` is paged and returns `{ items, total, page, pageSize }`, not a bare array. `/customers`, `/quotations`, `/invoices`, `/dispatches` still return arrays and are paged client-side.

## Frontend quirks
- **Nuxt 4 layout**: app code lives under `frontend/app/` (the srcDir). The `~` / `@` alias resolves to `frontend/app/`, so global CSS registered in `nuxt.config.ts` as `~/assets/css/main.css` must physically sit at `frontend/app/assets/css/main.css`.
- SPA only (`ssr: false`). Tailwind v4 via the `@tailwindcss/vite` plugin — there is no `tailwind.config.js`.
- **Design system**: all tokens are Tailwind v4 `@theme` custom properties in `app/assets/css/main.css` (Material-3 colour roles such as `surface-container-lowest` / `on-surface-variant`, a named spacing scale `space-md` / `gutter` / `margin`, and a type scale where family and size are applied as a pair, e.g. `font-label-sm text-label-sm`). Custom utilities: `elevation-1|2|3`, `focus-ring`, `tabular`. Never hardcode hex values in pages.
- **Use the shared components in `app/components/`** rather than re-writing markup: `AppButton`, `FormField`, `StatusBadge`, `AppModal` (dialog/drawer/`printable`), `DataTable` + `TableToolbar` + `TablePagination`, `KpiCard`, `PageHeader`, `StockProgressBar`, `EmptyState`, `LoadingSkeleton`, `PrintDocument`, `AppSidebar`, `AppTopbar`, `ToastHost`. `DataTable` overrides cells via dynamic slots (`#cell-<key>`), column defs live in `app/types/table.ts`.
- User feedback goes through `useToast()` (`toast.success/error/warning`), rendered by `ToastHost` mounted once in the layout. Do not add ad-hoc `message` refs.
- Icons are Material Symbols via `@nuxt/icon` (`<Icon name="material-symbols:add" />`), bundled locally. Fonts (Inter, Plus Jakarta Sans) are self-hosted by `@nuxt/fonts`. No CDN at runtime.
- Auth: JWT lives in `localStorage` and Nuxt `useState`, managed by `app/composables/useAuth.ts`; `app/middleware/auth.global.ts` redirects to `/login`. All HTTP goes through `app/composables/useApi.ts` (wraps `$fetch`, attaches the bearer token, logs out on 401).
- Pages in `app/pages/*`; the authenticated shell is `app/layouts/default.vue`; `login.vue` opts out with `definePageMeta({ layout: false })`. Print uses Tailwind `print:` variants plus a real `@page` A4 block with `print-color-adjust: exact` in `main.css`.
- Prefer the `angular-cli` MCP tools only for Angular repos — not here.

## Verify before done
```powershell
cd backend ; dotnet build
cd frontend ; npm run build
```
No test suites exist yet. Note `dotnet build` fails with a file-lock error if the API is still running — stop it first.
