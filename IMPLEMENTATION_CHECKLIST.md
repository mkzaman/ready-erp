# EGEL MVP — Implementation Checklist

## Backend (ASP.NET Core + EF Core + SQLite)
- [x] Scaffold Web API project
- [x] Add packages (EF Core SQLite, Design, JWT Bearer)
- [x] Domain entities (User, Product, Customer, Quotation, Invoice, Dispatch, StockMovement)
- [x] AppDbContext + SQLite config
- [x] DB seeding (admin/admin + sample data)
- [x] Auth (login + JWT)
- [x] Products module (CRUD + deactivate)
- [x] Customers module (CRUD)
- [x] Quotations module (create/list/view)
- [x] Convert quotation -> Invoice + Dispatch
- [x] Dispatch issue -> stock deduction + ledger
- [x] Invoices/Dispatches list/view
- [x] CORS for the SPA origin
- [x] Build passes

## Frontend (Nuxt 4, SPA)
- [x] Scaffold Nuxt app
- [x] Auth (login page + global middleware + bearer token in `useApi`)
- [x] Layout + navigation
- [x] Products page
- [x] Customers page
- [x] Quotations page (+ convert)
- [x] Invoices page (+ print view)
- [x] Dispatches page (+ print view)
- [x] Dashboard
- [x] API services wired
- [x] Build passes

---

## StockPulse redesign

Design source: Stitch project `389432496900708500` ("Precision Operations"), cobalt `#2563EB`.
Decisions: StockPulse visual language with existing page flows retained; real entities rather than
mocked UI; single `StockQuantity` with warehouse/location as metadata; desktop-first responsive.

### Phase 0 — Foundation
- [x] Port Stitch's Tailwind v3 JS config into Tailwind v4 `@theme` tokens in `app/assets/css/main.css`
      (48 M3 colour roles, named spacing scale, radii, 12-step type scale)
- [x] Custom utilities `elevation-1|2|3`, `focus-ring`, `tabular`
- [x] Real `@page` A4 print block with `print-color-adjust: exact`
- [x] `@nuxt/fonts` (self-hosted Inter + Plus Jakarta Sans) and `@nuxt/icon` (locally bundled Material Symbols)
- [x] Unblock the build: Node 16 -> 22.23.1 (Nuxt 4 needs >= 20), clean dependency reinstall
- [x] Deviation: `rounded-full` kept as a true pill; the Stitch config redefined it to 0.75rem,
      contradicting its own DESIGN.md. Use `rounded-2xl` for 0.75rem.

### Phase 1 — Component library (`app/components/`)
- [x] `AppButton` (4 variants, 3 sizes, icon/loading/link)
- [x] `FormField` (input/select/textarea, numeric coercion, error + hint, attr fallthrough)
- [x] `StatusBadge` (15 mapped statuses across 5 tones)
- [x] `AppModal` (dialog + drawer + `printable`, Esc, scroll lock)
- [x] `DataTable` + `TableToolbar` + `TablePagination` (dynamic `#cell-*` slots, selection, loading/empty)
- [x] `KpiCard` (dependency-free SVG sparkline), `PageHeader`, `StockProgressBar`
- [x] `EmptyState`, `LoadingSkeleton`, `ToastHost` + `useToast`
- [x] `app/types/table.ts` column/tab descriptors
- [x] Verified by a throwaway probe page exercising every prop and slot

### Phase 2 — Shell
- [x] `AppSidebar` (fixed rail, off-canvas drawer + scrim below `lg`, auto-close on navigation)
- [x] `AppTopbar` (blurred sticky bar, global product search with working Ctrl/⌘K)
- [x] `layouts/default.vue` rebuilt; all chrome `print:hidden`
- [x] `login.vue` restyled
- [x] Omitted deliberately: notification bell (no backing data)

### Phase 3 — Backend
- [x] `Category`, `Warehouse`, `Location` entities; `Product.Category` string -> `CategoryId` FK
- [x] `Product.WarehouseId` / `LocationId` / `ImagePath`
- [x] Nullable FKs with `OnDelete(SetNull)`; unique indexes on category name, warehouse code, `(WarehouseId, Code)`
- [x] `CategoriesController`, `WarehousesController` (+ `/{id}/locations`), `LocationsController`
- [x] `GET /api/products` paged + filtered (search, category, warehouse, location, status)
- [x] `POST|DELETE /api/products/{id}/image`, configurable `Storage:UploadPath`, static serving at `/uploads`
- [x] `GET /api/dashboard` real KPIs (stock value at cost, low/out counts, dispatch trend, reorder list)
- [x] Case-insensitive `ItemType` parsing; duplicate part number -> 409; bad references -> 400
- [x] Extended seed (warehouses, bins, categories, deliberate low-stock and out-of-stock rows)
- [x] `readyerp.db` deleted and rebuilt via `EnsureCreated`
- [x] Fixed: dashboard and product filter disagreed on "low stock" (`0 <= threshold` counted as low).
      One rule now: out = qty <= 0, low = qty > 0 && qty <= threshold.
- [x] `Issue` logic untouched — warehouse/location are metadata only

### Phase 4 — Pages
- [x] `products.vue` — KPI row, filter toolbar, status tabs, thumbnails, stock bars, bulk archive,
      server-side pagination, drawer form with image upload **(reviewed and approved)**
- [x] `dashboard.vue` — stock-health KPIs, dispatch sparkline, pending-dispatch callout, reorder table
- [x] `customers.vue` — table + drawer form with project-sites repeater
- [x] `quotations/index.vue` — status tabs with counts, convert confirmation dialog
- [x] `quotations/new.vue` — sectioned cards, line editor, short-stock warnings, **live totals preview**
- [x] `app/utils/pricing.ts` mirrors `PricingCalculator.cs`; verified against the API
- [x] `PrintDocument` shared by invoices and dispatches (was near-duplicate markup)
- [x] `invoices.vue` — VOIDED watermark, amount in words
- [x] `dispatches.vue` — issue confirmation, short-stock warnings surfaced via toast, PENDING watermark

### Phase 5 — Verification
- [x] `dotnet build` clean (0 errors, 0 warnings)
- [x] `npm run build` clean
- [x] End-to-end: login -> quotation -> convert -> issue -> stock deducted with shortfall warning
- [x] Stock unchanged on convert; deducted only on issue (business rule intact)
- [x] Document numbering shares one sequence (709 quote, 710 quote, 711 dispatch, 712 invoice)
- [x] Image upload: store, serve anonymously, replace (old file removed), delete
- [x] No legacy `#0f2b46` / `#c2410c` / `slate-*` values remain in any page
- [x] Docs updated (`AGENTS.md`, this file)

### Known gaps / not done
- [ ] No automated test suite (backend or frontend)
- [ ] Mobile: responsive but the three Stitch mobile-specific screens were not built
- [ ] No quotation edit endpoint (create and convert only) — pre-existing
- [ ] `NumberingService` is not concurrency-safe — pre-existing
- [ ] Auth remains demo-grade: SHA-256 passwords, hardcoded JWT secret
- [ ] Categories/warehouses/locations have API CRUD but no management UI yet
