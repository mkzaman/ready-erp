using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dispatches")]
public class DispatchesController : ControllerBase
{
    private readonly AppDbContext _db;
    public DispatchesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<object>> GetAll()
    {
        var items = await _db.Dispatches.Include(d => d.Customer).Include(d => d.Items)
            .OrderByDescending(d => d.Id).ToListAsync();
        return items.Select(d => new
        {
            d.Id, d.Number, d.Date, d.IsIssued,
            Customer = d.Customer?.Name ?? d.CustomerNameOverride,
            LineCount = d.Items.Count
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> Get(int id)
    {
        var d = await _db.Dispatches.Include(x => x.Customer).ThenInclude(c => c!.Sites)
            .Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id);
        return d is null ? NotFound() : Ok(new { Dispatch = d });
    }

    // Stock reduces only when a dispatch note is issued.
    [HttpPost("{id:int}/issue")]
    public async Task<ActionResult<object>> Issue(int id)
    {
        var dispatch = await _db.Dispatches.Include(d => d.Items).FirstOrDefaultAsync(d => d.Id == id);
        if (dispatch is null) return NotFound();
        if (dispatch.IsIssued) return BadRequest(new { message = "Dispatch already issued" });

        var warnings = new List<string>();

        foreach (var item in dispatch.Items.Where(i => i.ProductId.HasValue))
        {
            var product = await _db.Products.FindAsync(item.ProductId!.Value);
            if (product is null || product.Type == ItemType.Service) continue;

            var qty = (int)item.Quantity;
            if (product.StockQuantity < qty)
                warnings.Add($"{product.Name}: requested {qty}, available {product.StockQuantity}");

            // Warn-and-allow (proposal default Q3).
            product.StockQuantity -= qty;
            _db.StockMovements.Add(new StockMovement
            {
                ProductId = product.Id,
                QuantityChange = -qty,
                Reason = "Dispatch issued",
                SourceDocument = dispatch.Number
            });
        }

        dispatch.IsIssued = true;
        dispatch.IssuedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { dispatch.Id, dispatch.IsIssued, warnings });
    }
}
