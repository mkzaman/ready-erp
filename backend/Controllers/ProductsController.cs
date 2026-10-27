using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ProductsController(AppDbContext db) => _db = db;

    private static ProductDto Map(Product p) => new(
        p.Id, p.PartNumber, p.Name, p.Category, p.Unit, p.Type.ToString(),
        p.BuyingPrice, p.SellingPrice, p.StockQuantity, p.LowStockThreshold, p.IsActive);

    [HttpGet]
    public async Task<IEnumerable<ProductDto>> GetAll([FromQuery] string? search, [FromQuery] bool includeInactive = false)
    {
        var query = _db.Products.AsQueryable();
        if (!includeInactive) query = query.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.PartNumber.ToLower().Contains(s) ||
                p.Category.ToLower().Contains(s));
        }
        return await query.OrderBy(p => p.Name).Select(p => Map(p)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var p = await _db.Products.FindAsync(id);
        return p is null ? NotFound() : Map(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsert dto)
    {
        var product = new Product
        {
            PartNumber = dto.PartNumber,
            Name = dto.Name,
            Category = dto.Category,
            Unit = dto.Unit,
            Type = Enum.Parse<ItemType>(dto.Type),
            BuyingPrice = dto.BuyingPrice,
            SellingPrice = dto.SellingPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = product.Id }, Map(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsert dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.PartNumber = dto.PartNumber;
        product.Name = dto.Name;
        product.Category = dto.Category;
        product.Unit = dto.Unit;
        product.Type = Enum.Parse<ItemType>(dto.Type);
        product.BuyingPrice = dto.BuyingPrice;
        product.SellingPrice = dto.SellingPrice;
        product.StockQuantity = dto.StockQuantity;
        product.LowStockThreshold = dto.LowStockThreshold;

        await _db.SaveChangesAsync();
        return Map(product);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/adjust-stock")]
    public async Task<ActionResult<ProductDto>> AdjustStock(int id, StockAdjustRequest request)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        product.StockQuantity += request.QuantityChange;
        _db.StockMovements.Add(new StockMovement
        {
            ProductId = id,
            QuantityChange = request.QuantityChange,
            Reason = request.Reason,
            SourceDocument = request.SourceDocument ?? "Manual adjustment"
        });
        await _db.SaveChangesAsync();
        return Map(product);
    }
}
