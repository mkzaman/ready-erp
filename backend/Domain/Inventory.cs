namespace ReadyErp.Api.Domain;

/// <summary>
/// Product classification. Replaces the free-text <c>Product.Category</c> string
/// so the UI can offer a real filter list.
/// </summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Product> Products { get; set; } = new();
}

/// <summary>
/// A physical storage facility.
///
/// Note: stock is deliberately NOT tracked per warehouse. A product holds a
/// single <c>StockQuantity</c>; the warehouse and location record where that
/// stock is normally kept. Dispatch issuing is therefore unchanged.
/// </summary>
public class Warehouse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Location> Locations { get; set; } = new();
}

/// <summary>
/// A bin / rack / slot inside a <see cref="Warehouse"/>.
/// </summary>
public class Location
{
    public int Id { get; set; }

    public int WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public List<Product> Products { get; set; } = new();
}
