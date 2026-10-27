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
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly AppDbContext _db;
    public InvoicesController(AppDbContext db) => _db = db;

    private static TotalsDto Totals(Invoice inv)
    {
        var lines = inv.Items.Select(i => new LineInput(i.Quantity, i.UnitPrice, i.LineDiscountPercent, i.LineDiscountAmount));
        var t = PricingCalculator.Compute(lines, inv.DocumentDiscountPercent, inv.DocumentDiscountAmount,
            inv.VatPercent, inv.ServiceCharge, inv.WeightCharge, inv.DeliveryCharge);
        return new TotalsDto(t.SubTotal, t.Discount, t.Charges, t.Vat, t.GrandTotal, AmountInWords.Convert(t.GrandTotal));
    }

    [HttpGet]
    public async Task<IEnumerable<object>> GetAll()
    {
        var items = await _db.Invoices.Include(i => i.Customer).Include(i => i.Items)
            .OrderByDescending(i => i.Id).ToListAsync();
        return items.Select(i => new
        {
            i.Id, i.Number, i.Date, i.IsVoided,
            Customer = i.Customer?.Name ?? i.CustomerNameOverride,
            Totals = Totals(i)
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var inv = await _db.Invoices.Include(x => x.Customer).ThenInclude(c => c!.Sites)
            .Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        if (inv is null) return NotFound();
        return Ok(new { Invoice = inv, Totals = Totals(inv) });
    }

    [HttpPost("{id:int}/void")]
    public async Task<IActionResult> Void(int id)
    {
        var inv = await _db.Invoices.FindAsync(id);
        if (inv is null) return NotFound();
        inv.IsVoided = true;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
