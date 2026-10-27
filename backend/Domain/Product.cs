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
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public ItemType Type { get; set; } = ItemType.Product;
    public decimal BuyingPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
