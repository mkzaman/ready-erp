# E&G Engineering — MVP (Inventory, Invoicing & Quotation)

A client demo MVP: separate **Angular** frontend and **ASP.NET Core + EF Core (SQLite)** backend.

## Prerequisites
- .NET 10 SDK
- Node.js + npm

## Run the backend
```powershell
cd backend
dotnet run --urls http://localhost:5080
```
- SQLite DB (`readyerp.db`) is created and seeded automatically on first run.
- Seeded login: **admin / admin**
- Sample products, one customer are seeded for the demo.

## Run the frontend
```powershell
cd frontend
npm start
```
- Opens on http://localhost:4200
- API base URL is configured in `frontend/src/app/core/environment.ts`.

## Demo flow
1. Log in with `admin` / `admin`.
2. **Products** — add/edit catalogue items (product or service), adjust stock.
3. **Customers** — add customers with project sites.
4. **Quotations** — create a quotation, add line items, set VAT/charges.
5. On the quotations list, click **Convert** to generate a linked **Invoice** + **Dispatch**.
6. **Dispatches** — click **Issue** to reduce stock (warn-and-allow if short).
7. **Invoices / Dispatches** — open **View / Print** for a branded A4 print view (browser print → PDF).
8. **Dashboard** — counts and low-stock list.

## Scope notes (MVP)
- Phase 1 only (Phase 2 items such as payments, POs, richer reporting are out of scope).
- Single Admin role; auth is a simple seeded user with JWT.
- Document numbering uses one shared running sequence (`EGEL/<Type>/<Year>/<No>`).
- Pricing: charges added before a single document VAT %, grand total printed in words.

See `IMPLEMENTATION_CHECKLIST.md` for the full build status.
