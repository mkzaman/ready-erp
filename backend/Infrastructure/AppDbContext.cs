using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;

namespace ReadyErp.Api.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerSite> CustomerSites => Set<CustomerSite>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<DispatchItem> DispatchItems => Set<DispatchItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<DocumentCounter> DocumentCounters => Set<DocumentCounter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.PartNumber).IsUnique();

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("decimal(18,2)");
                }
            }
        }
    }
}
