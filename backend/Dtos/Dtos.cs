namespace ReadyErp.Api.Dtos;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Username, string Role);

public record ProductDto(
    int Id, string PartNumber, string Name, string Category, string Unit,
    string Type, decimal BuyingPrice, decimal SellingPrice, int StockQuantity,
    int? LowStockThreshold, bool IsActive);

public record ProductUpsert(
    string PartNumber, string Name, string Category, string Unit,
    string Type, decimal BuyingPrice, decimal SellingPrice, int StockQuantity,
    int? LowStockThreshold);

public record CustomerSiteDto(int Id, string SiteName, string? Address);
public record CustomerDto(
    int Id, string Name, string? BillingAddress, string? ContactPerson,
    string? Phone, string? Email, string? VatBin, bool IsActive,
    List<CustomerSiteDto> Sites);

public record CustomerUpsert(
    string Name, string? BillingAddress, string? ContactPerson,
    string? Phone, string? Email, string? VatBin,
    List<CustomerSiteUpsert> Sites);
public record CustomerSiteUpsert(string SiteName, string? Address);

public record QuotationItemDto(
    int? ProductId, string Description, string Unit, decimal Quantity,
    decimal UnitPrice, decimal LineDiscountPercent, decimal LineDiscountAmount, string? Remarks);

public record QuotationUpsert(
    int? CustomerId, string? CustomerNameOverride, string? WorkRef, string? ProjectSite,
    string? PoNo, DateTime? PoDate, string? JobRef, DateTime? ValidUntil,
    decimal DocumentDiscountPercent, decimal DocumentDiscountAmount, decimal VatPercent,
    decimal ServiceCharge, decimal WeightCharge, decimal DeliveryCharge,
    List<QuotationItemDto> Items);

public record TotalsDto(decimal SubTotal, decimal Discount, decimal Charges, decimal Vat, decimal GrandTotal, string GrandTotalInWords);

public record StockAdjustRequest(int QuantityChange, string Reason, string? SourceDocument);
