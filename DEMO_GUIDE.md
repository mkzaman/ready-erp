# Client Demo Guide — E&G Engineering ERP

A complete run-book for demonstrating the system. Read §1 and §2 before the client arrives.

**Total demo time: ~20 minutes.** Add 10 minutes for questions.

---

## 1. Setup — do this 30 minutes before

### 1.1 Reset to clean demo data

The database currently contains test documents from development. Reset it so the
demo starts from a predictable state and document numbers begin at a clean value.

```powershell
# Stop anything that is running first — the DB and the .exe will be locked.
Get-Process -Name "ReadyErp.Api","dotnet","node" -ErrorAction SilentlyContinue | Stop-Process -Force

cd C:\_workspace_\MyCode\ready.erp\backend
Remove-Item readyerp.db, readyerp.db-shm, readyerp.db-wal -Force -ErrorAction SilentlyContinue
Remove-Item wwwroot\uploads\* -Force -ErrorAction SilentlyContinue
```

The schema and seed data are recreated automatically on next start. There are no
migrations — deleting the file **is** the reset procedure.

### 1.2 Start the two services

They must run in **two separate terminals** and both must stay open.

```powershell
# Terminal 1 — API
cd C:\_workspace_\MyCode\ready.erp\backend
dotnet run --urls http://localhost:5080
```

```powershell
# Terminal 2 — Web
nvm use 22.23.1          # REQUIRED. Node 16 will fail with "node:util has no export parseArgs"
cd C:\_workspace_\MyCode\ready.erp\frontend
npm run dev
```

Wait for Terminal 2 to print `http://localhost:3000` before opening the browser.
First start takes 30–60 seconds.

### 1.3 Pre-flight check — do not skip

| # | Check | Expected |
|---|---|---|
| 1 | Open `http://localhost:3000` | Login screen, cobalt branding |
| 2 | Log in with `admin` / `admin` | Lands on Operations overview |
| 3 | Dashboard KPIs | Stock value **9.06M**, Low stock **1**, Out of stock **1** |
| 4 | Products page | 7 rows |
| 5 | Click "New quotation" in the header | Navigates to the builder |

If the dashboard shows zeros or an error toast, the API is not running or is on
the wrong port. See §6.

### 1.4 Browser preparation

- Use Chrome or Edge, **maximised**, zoom at 100%.
- Close other tabs — the topbar search uses **Ctrl+K**, which some extensions hijack.
- Have one product photo ready on the desktop for the image-upload step
  (JPG/PNG/WEBP/GIF, under 5 MB).
- Open an incognito/private window if you have browser extensions that inject UI.

---

## 2. The seeded data you will be demonstrating

Know these numbers — the client will ask.

**Products (7)**

| Part number | Name | Stock | Reorder at | State |
|---|---|---|---|---|
| DG-DEUTZ-100 | DEUTZ Diesel Genset 100kVA | 5 | 2 | In stock |
| CMP-SCREW-50 | Oil-injected Screw Compressor 50HP | 3 | 1 | In stock |
| CTRL-SMARTGEN | SmartGen Controller HGM6120 | 20 | 5 | In stock |
| SOLAR-ONGRID-5 | On-grid Solar System 5kW | 4 | 2 | In stock |
| **SP-FILTER-KIT** | Genset Service Filter Kit | **2** | 6 | **Low stock** |
| **SP-AVR-UNIVERSAL** | Universal AVR Module | **0** | 3 | **Out of stock** |
| SVC-INSTALL | Installation & Commissioning Service | — | — | Service (not tracked) |

The low-stock and out-of-stock rows are seeded deliberately so the alerts are
visible without having to fabricate anything live.

**Categories (5):** Diesel Gensets · Compressors · Synchronization & Automation ·
Spare Parts & Service · Solar Power

**Warehouses (2):** W1 Central Depot (bins A-01, A-02, B-01, B-02) ·
W2 Chattogram Yard (bins C-01, C-02)

**Customer (1):** Acme Industries Ltd., with one project site "Main Factory"

**Document numbering:** one shared running sequence across all document types.
The first document you create will be **EGEL/Quotation/2026/709**, the next
**/710**, and so on — quotations, invoices and dispatch notes all draw from the
same counter.

---

## 3. Demo script

### Act 1 — Dashboard (2 min)

**Do:** Log in. Stay on the Operations overview.

**Say:**
> "This is the operations cockpit. The top row is stock health — total inventory
> value at cost, and the two numbers that actually cost you money: items at their
> reorder point, and items that have run out."

**Point at:**
- **Stock value 9.06M** — valued at **cost**, not selling price, because that is
  the figure that matters for stock on hand.
- **Low stock 1** and **Out of stock 1** — two distinct states, never conflated.
- The **Reorder list** at the bottom, worst first, each row badged with its own state.

**Say:**
> "These numbers are live — they come from the same rule the inventory filters
> use, so the dashboard and the product list can never disagree."

---

### Act 2 — Inventory (5 min)

**Do:** Click **View inventory**.

**Walk through, in this order:**

1. **The KPI row** — same four figures, scoped to inventory.

2. **Status tabs** — click **Low**, then **Out**, then **Services**.
   > "Three states, plus services which are deliberately not stock-tracked —
   > you don't count an installation job in a warehouse."

3. **Search** — type `solar`.
   > "Search covers part number, product name, *and* category name."
   Clear it.

4. **Category / warehouse filters** — pick **Spare Parts & Service**, then
   **Central Depot**. Clear both.

5. **A table row** — point at the stock bar.
   > "The bar is relative to the reorder point, so you can see at a glance how
   > close something is to running out. Location shows the bin and the warehouse."

6. **Pagination** — change rows-per-page.
   > "Filtering and paging happen on the server, so this holds up at thousands of SKUs."

**Then create a product:**

- Click **New product**. The side drawer opens.
- **Upload** your prepared image.
- Part number: `DEMO-001`, Name: `Demo Control Panel`
- Category: **Synchronization & Automation**
- Warehouse: **Central Depot** → Location list populates → pick **B-01**
  > "The bin list is driven by the warehouse — you can't file something in a bin
  > that doesn't exist in that building."
- Selling price `45000`, Stock `4`, Reorder point `10`
- **Create product** → toast confirms, row appears with the thumbnail and a
  **Low stock** badge (4 is below the reorder point of 10).

> "One thing worth being explicit about: warehouse and bin are *where the stock
> normally sits*. Stock itself is a single figure per product. We can add
> per-warehouse balances later, but that is a bigger change and we didn't want to
> pretend it was there."

---

### Act 3 — Customers (1.5 min)

**Do:** Sidebar → **Customers** → **Edit** on Acme Industries.

**Say:**
> "Billing details, plus any number of project sites. Sites matter because a
> quotation is usually delivered somewhere other than the registered office."

Add a site: `Chattogram Plant` / `Port Access Road`. Save.

---

### Act 4 — Quotation builder (5 min) — the centrepiece

**Do:** Sidebar → **Quotations** → **New quotation**.

1. Customer: **Acme Industries Ltd.**
   > "Because we picked a real customer, Project Site becomes a dropdown of their
   > sites — including the one we just added."
   Select **Chattogram Plant**.

2. Work reference: `WR-2026-DEMO`, PO number: `PO-4471`.

3. **Line 1:** product **DEUTZ Diesel Genset 100kVA**.
   > "Picking a catalogue item fills the description, unit and current selling
   > price automatically — but every field stays editable for a one-off deal."
   Quantity `2`.

4. **Add line → Line 2:** product **Genset Service Filter Kit**, quantity `5`.
   > "Note the amber warning — we only have 2 in stock. It warns, it doesn't block,
   > because you absolutely do quote for things you'll order in."

5. **Add line → Line 3:** product **Installation & Commissioning Service**, quantity `1`.

6. **Watch the totals panel on the right the whole time.**
   Now set VAT to `15`, delivery charge `5000`, document discount `2` %.

   > "Totals update live as you type. Charges are added to the discounted subtotal
   > *before* VAT — that ordering is a real accounting decision, and it is
   > implemented identically on the client and the server. The server always
   > recalculates on save; the preview never becomes the source of truth."

7. **Save quotation** → lands on the list, new row **EGEL/Quotation/2026/709**, status **Draft**.

---

### Act 5 — The document flow (5 min) — the part that proves it's a system

**Do:** On the quotations list.

1. Click **Mark sent** → status becomes **Sent**.

2. Click **Convert**. A confirmation appears.
   Read it out:
   > "This creates the invoice and the dispatch note, *and* it says stock is not
   > deducted yet. That's the key control."

   Confirm. A toast confirms both documents were created.

3. **Point at the numbering:** the quotation was 709; open Dispatches and Invoices
   and show **EGEL/Dispatch/2026/710** and **EGEL/Invoice/2026/711**.
   > "One running sequence across all three document types, matching your current
   > manual practice."

4. **Sidebar → Invoices** → **View**.
   > "This is the printable document — letterhead, bill-to, the line table, totals,
   > amount in words, and signature blocks."
   Click **Print** → the browser print preview opens showing **A4 with proper
   margins and the colours preserved**. Close the preview.

5. **Sidebar → Dispatches.** Status is **Pending**.
   Click **View** → point at the **PENDING watermark** and the empty "Received"
   column.
   > "That column is blank on purpose — the driver gets it signed on delivery."
   Close.

6. **Before issuing**, go back to **Products** and show **SP-FILTER-KIT still at 2**.
   > "We've raised a quotation, an invoice and a dispatch note, and stock has not
   > moved. Nothing leaves the building on paperwork alone."

7. **Dispatches → Issue.** Read the confirmation, then confirm.
   An **amber warning toast** appears:
   `requested 5, available 2`.

   > "It issued, and it told you exactly what you're short. It doesn't silently
   > block the warehouse, and it doesn't silently hide the shortfall. Stock is now
   > negative, which is a visible, honest signal that something needs reconciling."

8. **Products** → SP-FILTER-KIT is now **-3** and badged **Out of stock**.
   Dashboard → **Dispatched today** tile now shows a figure and a trend line.

**Close with:**
> "That's the whole chain: quotation, acceptance, invoice, dispatch note, and
> stock moving exactly once — at the point goods physically leave."

---

### Act 6 — Optional, if there's interest (2 min)

- **Responsive:** narrow the browser window — sidebar collapses to a hamburger drawer.
- **Global search:** press **Ctrl+K**, type a part number, Enter.
- **Bulk actions:** tick two product rows → bulk Archive appears.

---

## 4. Printing

Print works from the invoice and dispatch note views only.

- Click **View**, then **Print**.
- In the print dialog set **Destination: Save as PDF**, **Paper: A4**,
  **Margins: Default**, and tick **Background graphics** if offered.
- The app chrome (sidebar, topbar, buttons) is automatically hidden.

**Rehearse this once before the demo** — printer dialogs differ between machines
and this is the step most likely to look clumsy live.

---

## 5. Questions you should expect, and honest answers

**"Can we track stock per warehouse?"**
> Not today. Stock is one figure per product; warehouse and bin record where it
> normally sits. Per-warehouse balances are a real change — it affects the
> dispatch logic — so we scoped it out rather than fake it.

**"Why did stock go negative?"**
> Deliberate. Blocking the warehouse because the system disagrees with reality
> causes workarounds. It issues, warns, and leaves a visible negative so it gets
> reconciled.

**"Can I edit a quotation after saving?"**
> Not in this build. Create and convert only. It's a known gap, straightforward to add.

**"Is this secure / can we have multiple users?"**
> This is a demo build — single seeded admin, simplified password hashing. Real
> authentication, user accounts and roles are a known, well-understood piece of work.
> *(Don't oversell. Move on.)*

**"Where are the images stored?"**
> On the server's disk, with the path configurable. When we containerise, images
> and the database sit on one mounted volume.

**"Can we manage categories and warehouses in the UI?"**
> The API supports full create/edit for categories, warehouses and bins; the
> management screens aren't built yet. Adding them is small.

---

## 6. If something goes wrong

| Symptom | Cause | Fix |
|---|---|---|
| Dashboard all zeros, red error toast | API not running | Check Terminal 1; restart `dotnet run --urls http://localhost:5080` |
| `node:util has no export parseArgs` | Node 16 active | `nvm use 22.23.1`, then `npm run dev` |
| Login rejected | DB deleted while API running | Stop API, delete `readyerp.db*`, restart |
| `dotnet build` fails, file locked | API still running | Stop it first |
| Ctrl+K does nothing | Browser extension captured it | Use the topbar search box directly |
| Images don't appear | `wwwroot/uploads` cleared while API ran | Re-upload; or restart the API |
| Port 3000 or 5080 in use | Orphaned process | `Get-Process -Name node,dotnet \| Stop-Process -Force` |

**Nuclear reset — takes about 90 seconds:**

```powershell
Get-Process -Name "ReadyErp.Api","dotnet","node" -ErrorAction SilentlyContinue | Stop-Process -Force
cd C:\_workspace_\MyCode\ready.erp\backend
Remove-Item readyerp.db*, wwwroot\uploads\* -Force -ErrorAction SilentlyContinue
# then restart both terminals as in §1.2
```

---

## 7. Between demos

If you are running the demo more than once, **reset the database between runs**
(§1.1). Otherwise the second client sees leftover documents, negative stock on
SP-FILTER-KIT, and document numbers starting at 713 instead of 709.

---

## 8. Do not demo these

Not broken — simply not built. Avoid opening a thread you have to close awkwardly.

- Category / warehouse / bin management screens (API only)
- Editing or deleting a saved quotation
- Voiding an invoice from the UI (endpoint exists, no button)
- User management, roles, password change
- Reports, exports, PDF generation beyond browser print
- The three mobile-specific layouts from the design system
- Any workflow with more than one user

---

## 9. Quick reference

| Item | Value |
|---|---|
| Web | `http://localhost:3000` |
| API | `http://localhost:5080` |
| Login | `admin` / `admin` |
| Node | 22.23.1 (`nvm use 22.23.1`) |
| Database | `backend\readyerp.db` (SQLite, delete to reset) |
| Uploads | `backend\wwwroot\uploads` |
| First document number | `EGEL/Quotation/2026/709` |

**The one-sentence pitch, if you only get 30 seconds:**
> "Quotation to invoice to dispatch note on a single document sequence, with stock
> deducted exactly once — when goods actually leave the building."
