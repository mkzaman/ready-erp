namespace ReadyErp.Api.Domain;

public enum ItemType
{
    Product = 0,
    Service = 1
}

public class Product
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public ItemType Type { get; set; } = ItemType.Product;
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Replaces the former free-text category string.</summary>
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    /// <summary>
    /// Where this item is normally held. Metadata only — stock is a single
    /// figure on the product, not a per-warehouse balance.
    /// </summary>
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public int? LocationId { get; set; }
    public Location? Location { get; set; }

    /// <summary>
    /// Public URL of the uploaded thumbnail, e.g. <c>/uploads/ab12.jpg</c>.
    /// Null when no image has been uploaded.
    /// </summary>
    public string? ImagePath { get; set; }
}
