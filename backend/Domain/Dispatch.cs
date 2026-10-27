namespace ReadyErp.Api.Domain;

public class Dispatch
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerNameOverride { get; set; }

    public int? QuotationId { get; set; }

    public string? WorkRef { get; set; }
    public string? ProjectSite { get; set; }
    public string? PoNo { get; set; }
    public DateTime? PoDate { get; set; }
    public string? JobRef { get; set; }

    public bool IsIssued { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<DispatchItem> Items { get; set; } = new();
}

public class DispatchItem
{
    public int Id { get; set; }
    public int DispatchId { get; set; }
    public Dispatch? Dispatch { get; set; }

    public int? ProductId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public decimal Quantity { get; set; }
}
