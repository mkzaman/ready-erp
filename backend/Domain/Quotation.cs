namespace ReadyErp.Api.Domain;

public enum QuotationStatus
{
    Draft = 0,
    Sent = 1,
    Accepted = 2,
    Rejected = 3,
    Expired = 4
}

public class Quotation
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? CustomerNameOverride { get; set; }

    public string? WorkRef { get; set; }
    public string? ProjectSite { get; set; }
    public string? PoNo { get; set; }
    public DateTime? PoDate { get; set; }
    public string? JobRef { get; set; }

    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

    public decimal DocumentDiscountPercent { get; set; }
    public decimal DocumentDiscountAmount { get; set; }
    public decimal VatPercent { get; set; }
    public decimal ServiceCharge { get; set; }
    public decimal WeightCharge { get; set; }
    public decimal DeliveryCharge { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<QuotationItem> Items { get; set; } = new();
}

public class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public Quotation? Quotation { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public string Description { get; set; } = string.Empty;
    public string Unit { get; set; } = "Pcs";
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineDiscountPercent { get; set; }
    public decimal LineDiscountAmount { get; set; }
    public string? Remarks { get; set; }
}
