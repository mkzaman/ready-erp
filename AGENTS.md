# AGENTS.md

Client-demo MVP for E&G Engineering: inventory, quotations, invoices, dispatch. Two independent apps — `backend/` (ASP.NET Core + EF Core + SQLite) and `frontend/` (Angular). Not a monorepo; no root build. The source requirement is `E&G_Engineering_Proposal.pdf`; `IMPLEMENTATION_CHECKLIST.md` tracks scope.

## Run (needs two terminals)
```powershell
cd backend ; dotnet run --urls http://localhost:5080   # API
cd frontend ; npm start                                 # http://localhost:4200
```
- Login: **admin / admin** (seeded). No registration, no email flows.
- Frontend expects the API at `http://localhost:5080/api` — hardcoded in `frontend/src/app/core/environment.ts` (not Angular's `environments/`). CORS in `backend/Program.cs` only allows `http://localhost:4200`. Change both together if a port moves.

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
- Standalone components only (no NgModules). Use the `angular-cli` MCP tools for Angular work, not raw `ng`.
- Features under `src/app/features/*`; shared auth/API/models under `src/app/core/*`. All HTTP goes through `core/api.service.ts`; JWT is attached by `core/auth.interceptor.ts`.

## Verify before done
```powershell
cd backend ; dotnet build
cd frontend ; npx ng build
```
No test suites exist yet.
