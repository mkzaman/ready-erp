using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;
using ReadyErp.Api.Services;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quotations")]
public class QuotationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly NumberingService _numbering;

    public QuotationsController(AppDbContext db, NumberingService numbering)
    {
        _db = db;
        _numbering = numbering;
    }

    private static TotalsDto Totals(Quotation q)
    {
        var lines = q.Items.Select(i => new LineInput(i.Quantity, i.UnitPrice, i.LineDiscountPercent, i.LineDiscountAmount));
        var t = PricingCalculator.Compute(lines, q.DocumentDiscountPercent, q.DocumentDiscountAmount,
            q.VatPercent, q.ServiceCharge, q.WeightCharge, q.DeliveryCharge);
        return new TotalsDto(t.SubTotal, t.Discount, t.Charges, t.Vat, t.GrandTotal, AmountInWords.Convert(t.GrandTotal));
    }

    [HttpGet]
    public async Task<IEnumerable<object>> GetAll()
    {
        var items = await _db.Quotations.Include(q => q.Customer).Include(q => q.Items)
            .OrderByDescending(q => q.Id).ToListAsync();
        return items.Select(q => new
        {
            q.Id, q.Number, q.Date, Status = q.Status.ToString(),
            Customer = q.Customer?.Name ?? q.CustomerNameOverride,
            Totals = Totals(q)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var q = await _db.Quotations.Include(x => x.Customer).Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();
        return Ok(new { Quotation = q, Totals = Totals(q) });
    }

    [HttpPost]
    public async Task<ActionResult> Create(QuotationUpsert dto)
    {
        var q = new Quotation
        {
            Number = _numbering.Next("Quotation"),
            CustomerId = dto.CustomerId,
            CustomerNameOverride = dto.CustomerNameOverride,
            WorkRef = dto.WorkRef,
            ProjectSite = dto.ProjectSite,
            PoNo = dto.PoNo,
            PoDate = dto.PoDate,
            JobRef = dto.JobRef,
            ValidUntil = dto.ValidUntil,
            DocumentDiscountPercent = dto.DocumentDiscountPercent,
            DocumentDiscountAmount = dto.DocumentDiscountAmount,
            VatPercent = dto.VatPercent,
            ServiceCharge = dto.ServiceCharge,
            WeightCharge = dto.WeightCharge,
            DeliveryCharge = dto.DeliveryCharge,
            Items = dto.Items.Select(i => new QuotationItem
            {
                ProductId = i.ProductId,
                Description = i.Description,
                Unit = i.Unit,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineDiscountPercent = i.LineDiscountPercent,
                LineDiscountAmount = i.LineDiscountAmount,
                Remarks = i.Remarks
            }).ToList()
        };
        _db.Quotations.Add(q);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = q.Id }, new { q.Id });
    }

    [HttpPost("{id:int}/status/{status}")]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        var q = await _db.Quotations.FindAsync(id);
        if (q is null) return NotFound();
        if (!Enum.TryParse<QuotationStatus>(status, true, out var parsed))
            return BadRequest(new { message = "Invalid status" });
        q.Status = parsed;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/convert")]
    public async Task<ActionResult> Convert(int id)
    {
        var q = await _db.Quotations.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (q is null) return NotFound();

        q.Status = QuotationStatus.Accepted;

        var dispatch = new Dispatch
        {
            Number = _numbering.Next("Dispatch"),
            CustomerId = q.CustomerId,
            CustomerNameOverride = q.CustomerNameOverride,
            QuotationId = q.Id,
            WorkRef = q.WorkRef,
            ProjectSite = q.ProjectSite,
            PoNo = q.PoNo,
            PoDate = q.PoDate,
            JobRef = q.JobRef,
            Items = q.Items.Select(i => new DispatchItem
            {
                ProductId = i.ProductId,
                Description = i.Description,
                Unit = i.Unit,
                Quantity = i.Quantity
            }).ToList()
        };
        _db.Dispatches.Add(dispatch);
        await _db.SaveChangesAsync();

        var invoice = new Invoice
        {
            Number = _numbering.Next("Invoice"),
            CustomerId = q.CustomerId,
            CustomerNameOverride = q.CustomerNameOverride,
            QuotationId = q.Id,
            DispatchId = dispatch.Id,
            WorkRef = q.WorkRef,
            ProjectSite = q.ProjectSite,
            PoNo = q.PoNo,
            PoDate = q.PoDate,
            JobRef = q.JobRef,
            DocumentDiscountPercent = q.DocumentDiscountPercent,
            DocumentDiscountAmount = q.DocumentDiscountAmount,
            VatPercent = q.VatPercent,
            ServiceCharge = q.ServiceCharge,
            WeightCharge = q.WeightCharge,
            DeliveryCharge = q.DeliveryCharge,
            Items = q.Items.Select(i => new InvoiceItem
            {
                ProductId = i.ProductId,
                Description = i.Description,
                Unit = i.Unit,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineDiscountPercent = i.LineDiscountPercent,
                LineDiscountAmount = i.LineDiscountAmount,
                Remarks = i.Remarks
            }).ToList()
        };
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        return Ok(new { invoiceId = invoice.Id, dispatchId = dispatch.Id });
    }
}
