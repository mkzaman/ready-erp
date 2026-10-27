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
- [x] CORS for Angular
- [x] Build passes

## Frontend (Angular)
- [x] Scaffold Angular app
- [x] Auth (login page + guard + interceptor)
- [x] Layout + navigation
- [x] Products page
- [x] Customers page
- [x] Quotations page (+ convert)
- [x] Invoices page (+ print view)
- [x] Dispatches page (+ print view)
- [x] Dashboard
- [x] API services wired
- [x] Build passes

## Final
- [x] End-to-end smoke test
- [x] Run instructions documented
