namespace ReadyErp.Api.Dtos;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string Username, string Role);

/// <summary>Envelope for server-side paged list endpoints.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

/// <summary>Derived stock state, so the UI does not re-implement the rule.</summary>
public enum StockState { InStock, LowStock, OutOfStock, NotTracked }

public record ProductDto(
    int Id, string PartNumber, string Name, string Unit,
    string Type, decimal BuyingPrice, decimal SellingPrice, int StockQuantity,
    int? LowStockThreshold, bool IsActive,
    int? CategoryId, string? CategoryName,
    int? WarehouseId, string? WarehouseName,
    int? LocationId, string? LocationCode,
    string? ImagePath, string StockState);

public record ProductUpsert(
    string PartNumber, string Name, string Unit,
    string Type, decimal BuyingPrice, decimal SellingPrice, int StockQuantity,
    int? LowStockThreshold,
    int? CategoryId, int? WarehouseId, int? LocationId);

public record CategoryDto(int Id, string Name, string? Description, bool IsActive, int ProductCount);
public record CategoryUpsert(string Name, string? Description);

public record WarehouseDto(int Id, string Code, string Name, string? Address, bool IsActive, int LocationCount);
public record WarehouseUpsert(string Code, string Name, string? Address);

public record LocationDto(int Id, int WarehouseId, string WarehouseName, string Code, string? Description, bool IsActive);
public record LocationUpsert(int WarehouseId, string Code, string? Description);

/// <summary>Aggregate figures for the dashboard and the products KPI row.</summary>
public record DashboardStatsDto(
    int ProductCount, int CustomerCount, int QuotationCount, int InvoiceCount, int DispatchCount,
    int LowStockCount, int OutOfStockCount, decimal TotalStockValue,
    int UnitsDispatchedToday, int DispatchesPendingIssue,
    /// <summary>
    /// Everything at or below its reorder point, worst first. Includes
    /// out-of-stock items, so each entry carries its own StockState.
    /// </summary>
    IReadOnlyList<LowStockItemDto> ReorderList,
    /// <summary>Units dispatched per day for the last 7 days, oldest first.</summary>
    IReadOnlyList<int> DispatchTrend);

public record LowStockItemDto(
    int Id, string PartNumber, string Name, int StockQuantity, int? LowStockThreshold,
    string? CategoryName, string StockState);

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
