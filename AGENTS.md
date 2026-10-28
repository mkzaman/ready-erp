# AGENTS.md

Client-demo MVP for E&G Engineering: inventory, quotations, invoices, dispatch. Two independent apps — `backend/` (ASP.NET Core + EF Core + SQLite) and `frontend/` (Nuxt 3, SPA mode). Not a monorepo; no root build. The source requirement is `E&G_Engineering_Proposal.pdf`; `IMPLEMENTATION_CHECKLIST.md` tracks scope.

## Run (needs two terminals)
```powershell
cd backend ; dotnet run --urls http://localhost:5080   # API
cd frontend ; npm run dev                               # http://localhost:3000
```
- Login: **admin / admin** (seeded). No registration, no email flows.
- Frontend expects the API at `http://localhost:5080/api` — set via `runtimeConfig.public.apiBase` in `frontend/nuxt.config.ts`. CORS in `backend/Program.cs` only allows `http://localhost:3000`. Change both together if a port moves.

## Backend quirks
- **No EF migrations.** Schema is created via `DbContext.Database.EnsureCreated()` in `Infrastructure/DbSeeder.cs`, which also seeds admin + sample data. To change the schema, delete `backend/readyerp.db` (gitignored) and re-run; do not scaffold migrations unless you intend to switch the whole approach.
- JSON uses `ReferenceHandler.IgnoreCycles` (set in `Program.cs`) because controllers return EF entities with navigation cycles (e.g. `Customer` ↔ `Sites`). Keep it, or return DTOs instead.
- Passwords are SHA-256 (`Infrastructure/PasswordHasher.cs`) and the JWT secret is hardcoded in `Services/TokenService.cs` — demo-only, not production auth.
- Structure is layered by folder: `Domain/`, `Infrastructure/`, `Services/`, `Dtos/`, `Controllers/`.
- Business rules that are easy to break:
  - Document numbers share ONE running sequence across quotations/invoices/dispatches (`Services/NumberingService.cs`), format `EGEL/<Type>/<Year>/<No>`.
  - Stock only decrements when a dispatch is **issued** (`DispatchesController.Issue`), never on quote/invoice; services (`ItemType.Service`) are skipped. Short stock = warn-and-allow, not block.
  - Totals/VAT/amount-in-words live in `Services/PricingCalculator.cs` and `AmountInWords.cs`; charges are added before VAT.

## Frontend quirks
- **Nuxt 4 layout**: app code lives under `frontend/app/` (the srcDir). The `~` / `@` alias resolves to `frontend/app/`, so global CSS registered in `nuxt.config.ts` as `~/assets/css/main.css` must physically sit at `frontend/app/assets/css/main.css`.
- SPA only (`ssr: false`). Tailwind v4 via the `@tailwindcss/vite` plugin + `@import 'tailwindcss'` in `app/assets/css/main.css` — there is no `tailwind.config.js`.
- Auth: JWT lives in `localStorage` and Nuxt `useState`, managed by `app/composables/useAuth.ts`; `app/middleware/auth.global.ts` redirects to `/login`. All HTTP goes through `app/composables/useApi.ts` (wraps `$fetch`, attaches the bearer token, logs out on 401).
- Pages in `app/pages/*`; the authenticated shell is `app/layouts/default.vue`; `login.vue` opts out with `definePageMeta({ layout: false })`. Print views use Tailwind `print:` variants.
- Prefer the `angular-cli` MCP tools only for Angular repos — not here.

## Verify before done
```powershell
cd backend ; dotnet build
cd frontend ; npm run build
```
No test suites exist yet.
