using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    /// <summary>Reason string written by DispatchesController.Issue.</summary>
    private const string DispatchReason = "Dispatch issued";

    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<DashboardStatsDto> Get()
    {
        var active = _db.Products.Where(p => p.IsActive && p.Type != ItemType.Service);

        // Single source of truth for the state rule, matching
        // ProductsController's status filter exactly:
        //   out  = qty <= 0
        //   low  = qty > 0 and qty <= threshold
        var lowStockCount = await active
            .CountAsync(p => p.StockQuantity > 0 &&
                             p.LowStockThreshold != null &&
                             p.StockQuantity <= p.LowStockThreshold);

        var outOfStockCount = await active.CountAsync(p => p.StockQuantity <= 0);

        // Actionable reorder list: everything at or below its reorder point,
        // including items that have already run out. Each row reports its own
        // state so the UI can badge it correctly.
        var reorderList = await active
            .Where(p => p.LowStockThreshold != null && p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockItemDto(
                p.Id, p.PartNumber, p.Name, p.StockQuantity, p.LowStockThreshold,
                p.Category != null ? p.Category.Name : null,
                p.StockQuantity <= 0 ? nameof(StockState.OutOfStock) : nameof(StockState.LowStock)))
            .ToListAsync();

        // Inventory valued at cost, which is the figure that matters for a
        // stock-on-hand number. Services are excluded (not stock-tracked).
        var totalStockValue = await active.SumAsync(p => p.BuyingPrice * p.StockQuantity);

        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-6);

        // Dispatch movements are stored as negative quantities.
        var recentMovements = await _db.StockMovements
            .Where(m => m.Reason == DispatchReason && m.CreatedAt >= weekStart)
            .Select(m => new { m.CreatedAt, m.QuantityChange })
            .ToListAsync();

        var trend = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var day = weekStart.AddDays(offset);
                return recentMovements
                    .Where(m => m.CreatedAt.Date == day)
                    .Sum(m => Math.Abs(m.QuantityChange));
            })
            .ToList();

        return new DashboardStatsDto(
            ProductCount: await _db.Products.CountAsync(p => p.IsActive),
            CustomerCount: await _db.Customers.CountAsync(c => c.IsActive),
            QuotationCount: await _db.Quotations.CountAsync(),
            InvoiceCount: await _db.Invoices.CountAsync(i => !i.IsVoided),
            DispatchCount: await _db.Dispatches.CountAsync(),
            LowStockCount: lowStockCount,
            OutOfStockCount: outOfStockCount,
            TotalStockValue: totalStockValue,
            UnitsDispatchedToday: trend[^1],
            DispatchesPendingIssue: await _db.Dispatches.CountAsync(d => !d.IsIssued),
            ReorderList: reorderList,
            DispatchTrend: trend);
    }
}
