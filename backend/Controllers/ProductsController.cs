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
    private readonly StorageOptions _storage;

    public ProductsController(AppDbContext db, StorageOptions storage)
    {
        _db = db;
        _storage = storage;
    }

    /// <summary>
    /// Derives the stock state. Services are not stock-tracked, so they report
    /// <c>NotTracked</c> rather than pretending to be out of stock.
    /// </summary>
    private static string StateOf(Product p) =>
        p.Type == ItemType.Service ? nameof(StockState.NotTracked)
        : p.StockQuantity <= 0 ? nameof(StockState.OutOfStock)
        : p.LowStockThreshold != null && p.StockQuantity <= p.LowStockThreshold ? nameof(StockState.LowStock)
        : nameof(StockState.InStock);

    /// <summary>
    /// Turns the stored relative image path into an absolute URL, since the
    /// SPA runs on a different origin than the API.
    /// </summary>
    private string? AbsoluteImage(string? relative) =>
        string.IsNullOrWhiteSpace(relative)
            ? null
            : $"{Request.Scheme}://{Request.Host}{relative}";

    private ProductDto Map(Product p) => new(
        p.Id, p.PartNumber, p.Name, p.Unit, p.Type.ToString(),
        p.BuyingPrice, p.SellingPrice, p.StockQuantity, p.LowStockThreshold, p.IsActive,
        p.CategoryId, p.Category?.Name,
        p.WarehouseId, p.Warehouse?.Name,
        p.LocationId, p.Location?.Code,
        AbsoluteImage(p.ImagePath),
        StateOf(p));

    /// <summary>
    /// Paged, filtered product list.
    /// </summary>
    /// <param name="search">Matches part number, name or category name.</param>
    /// <param name="status">InStock | LowStock | OutOfStock | NotTracked.</param>
    [HttpGet]
    public async Task<PagedResult<ProductDto>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? warehouseId,
        [FromQuery] int? locationId,
        [FromQuery] string? status,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Warehouse)
            .Include(p => p.Location)
            .AsQueryable();

        if (!includeInactive) query = query.Where(p => p.IsActive);
        if (categoryId is not null) query = query.Where(p => p.CategoryId == categoryId);
        if (warehouseId is not null) query = query.Where(p => p.WarehouseId == warehouseId);
        if (locationId is not null) query = query.Where(p => p.LocationId == locationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.PartNumber.ToLower().Contains(s) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(s)));
        }

        // Status is applied in SQL so paging stays correct.
        query = status?.ToLowerInvariant() switch
        {
            "instock" => query.Where(p => p.Type != ItemType.Service && p.StockQuantity > 0 &&
                (p.LowStockThreshold == null || p.StockQuantity > p.LowStockThreshold)),
            "lowstock" => query.Where(p => p.Type != ItemType.Service && p.StockQuantity > 0 &&
                p.LowStockThreshold != null && p.StockQuantity <= p.LowStockThreshold),
            "outofstock" => query.Where(p => p.Type != ItemType.Service && p.StockQuantity <= 0),
            "nottracked" => query.Where(p => p.Type == ItemType.Service),
            _ => query
        };

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ProductDto>(items.Select(Map).ToList(), total, page, pageSize);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        var p = await _db.Products
            .Include(x => x.Category)
            .Include(x => x.Warehouse)
            .Include(x => x.Location)
            .FirstOrDefaultAsync(x => x.Id == id);

        return p is null ? NotFound() : Map(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductUpsert dto)
    {
        if (!Enum.TryParse<ItemType>(dto.Type, ignoreCase: true, out var type))
            return BadRequest(new { message = $"Unknown item type '{dto.Type}'." });

        if (await _db.Products.AnyAsync(p => p.PartNumber == dto.PartNumber))
            return Conflict(new { message = $"Part number '{dto.PartNumber}' already exists." });

        var error = await ValidateReferences(dto);
        if (error is not null) return BadRequest(new { message = error });

        var product = new Product
        {
            PartNumber = dto.PartNumber,
            Name = dto.Name,
            Unit = dto.Unit,
            Type = type,
            BuyingPrice = dto.BuyingPrice,
            SellingPrice = dto.SellingPrice,
            StockQuantity = dto.StockQuantity,
            LowStockThreshold = dto.LowStockThreshold,
            CategoryId = dto.CategoryId,
            WarehouseId = dto.WarehouseId,
            LocationId = dto.LocationId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        await LoadReferences(product);

        return CreatedAtAction(nameof(Get), new { id = product.Id }, Map(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, ProductUpsert dto)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (!Enum.TryParse<ItemType>(dto.Type, ignoreCase: true, out var type))
            return BadRequest(new { message = $"Unknown item type '{dto.Type}'." });

        if (await _db.Products.AnyAsync(p => p.PartNumber == dto.PartNumber && p.Id != id))
            return Conflict(new { message = $"Part number '{dto.PartNumber}' already exists." });

        var error = await ValidateReferences(dto);
        if (error is not null) return BadRequest(new { message = error });

        product.PartNumber = dto.PartNumber;
        product.Name = dto.Name;
        product.Unit = dto.Unit;
        product.Type = type;
        product.BuyingPrice = dto.BuyingPrice;
        product.SellingPrice = dto.SellingPrice;
        product.StockQuantity = dto.StockQuantity;
        product.LowStockThreshold = dto.LowStockThreshold;
        product.CategoryId = dto.CategoryId;
        product.WarehouseId = dto.WarehouseId;
        product.LocationId = dto.LocationId;

        await _db.SaveChangesAsync();
        await LoadReferences(product);

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

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();
        product.IsActive = true;
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
        await LoadReferences(product);

        return Map(product);
    }

    /// <summary>Uploads or replaces the product thumbnail.</summary>
    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ProductDto>> UploadImage(int id, IFormFile file)
    {
        if (!_storage.IsWritable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                message = "Image storage is not available on this server. " +
                          "Set Storage__UploadPath to a writable directory."
            });
        }

        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file was uploaded." });

        if (file.Length > _storage.MaxBytes)
            return BadRequest(new { message = $"File exceeds the {_storage.MaxBytes / 1024 / 1024} MB limit." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!StorageOptions.AllowedExtensions.Contains(extension))
            return BadRequest(new
            {
                message = $"Unsupported file type '{extension}'. Allowed: {string.Join(", ", StorageOptions.AllowedExtensions)}."
            });

        // Generated name: never trust the client-supplied filename on disk.
        var fileName = $"product-{id}-{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storage.UploadPath, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        DeleteImageFile(product.ImagePath);

        product.ImagePath = $"{_storage.RequestPath}/{fileName}";
        await _db.SaveChangesAsync();
        await LoadReferences(product);

        return Map(product);
    }

    [HttpDelete("{id:int}/image")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null) return NotFound();

        DeleteImageFile(product.ImagePath);
        product.ImagePath = null;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private void DeleteImageFile(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return;

        var name = Path.GetFileName(relativePath);
        var path = Path.Combine(_storage.UploadPath, name);

        // Guard against path traversal via a tampered stored value.
        if (!path.StartsWith(_storage.UploadPath, StringComparison.Ordinal)) return;
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    /// <summary>Returns an error message when a referenced id does not exist.</summary>
    private async Task<string?> ValidateReferences(ProductUpsert dto)
    {
        if (dto.CategoryId is not null && !await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId))
            return "The selected category no longer exists.";

        if (dto.WarehouseId is not null && !await _db.Warehouses.AnyAsync(w => w.Id == dto.WarehouseId))
            return "The selected warehouse no longer exists.";

        if (dto.LocationId is not null)
        {
            var location = await _db.Locations.FirstOrDefaultAsync(l => l.Id == dto.LocationId);
            if (location is null) return "The selected location no longer exists.";

            if (dto.WarehouseId is not null && location.WarehouseId != dto.WarehouseId)
                return "The selected location belongs to a different warehouse.";
        }

        return null;
    }

    private async Task LoadReferences(Product product)
    {
        await _db.Entry(product).Reference(p => p.Category).LoadAsync();
        await _db.Entry(product).Reference(p => p.Warehouse).LoadAsync();
        await _db.Entry(product).Reference(p => p.Location).LoadAsync();
    }
}
