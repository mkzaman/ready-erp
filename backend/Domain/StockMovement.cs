namespace ReadyErp.Api.Domain;

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int QuantityChange { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? SourceDocument { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class DocumentCounter
{
    public int Id { get; set; }
    public int Year { get; set; }
    public int LastNumber { get; set; }
}
