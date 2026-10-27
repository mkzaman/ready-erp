using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<object> Get()
    {
        var lowStock = await _db.Products
            .Where(p => p.IsActive && p.LowStockThreshold != null && p.StockQuantity <= p.LowStockThreshold)
            .Select(p => new { p.Name, p.PartNumber, p.StockQuantity, p.LowStockThreshold })
            .ToListAsync();

        return new
        {
            ProductCount = await _db.Products.CountAsync(p => p.IsActive),
            CustomerCount = await _db.Customers.CountAsync(c => c.IsActive),
            QuotationCount = await _db.Quotations.CountAsync(),
            InvoiceCount = await _db.Invoices.CountAsync(),
            DispatchCount = await _db.Dispatches.CountAsync(),
            LowStock = lowStock
        };
    }
}
