using ReadyErp.Api.Domain;

namespace ReadyErp.Api.Infrastructure;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        db.Database.EnsureCreated();

        if (!db.Users.Any())
        {
            db.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = PasswordHasher.Hash("admin"),
                Role = "Admin"
            });
        }

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { PartNumber = "DG-DEUTZ-100", Name = "DEUTZ Diesel Genset 100kVA", Category = "Diesel Gensets", Unit = "Pcs", Type = ItemType.Product, BuyingPrice = 1200000, SellingPrice = 1500000, StockQuantity = 5, LowStockThreshold = 2 },
                new Product { PartNumber = "CMP-SCREW-50", Name = "Oil-injected Screw Compressor 50HP", Category = "Compressors", Unit = "Pcs", Type = ItemType.Product, BuyingPrice = 450000, SellingPrice = 600000, StockQuantity = 3, LowStockThreshold = 1 },
                new Product { PartNumber = "CTRL-SMARTGEN", Name = "SmartGen Controller HGM6120", Category = "Synchronization & Automation", Unit = "Pcs", Type = ItemType.Product, BuyingPrice = 25000, SellingPrice = 38000, StockQuantity = 20, LowStockThreshold = 5 },
                new Product { PartNumber = "SVC-INSTALL", Name = "Installation & Commissioning Service", Category = "Spare Parts & Service", Unit = "Job", Type = ItemType.Service, BuyingPrice = 0, SellingPrice = 50000 },
                new Product { PartNumber = "SOLAR-ONGRID-5", Name = "On-grid Solar System 5kW", Category = "Solar Power", Unit = "Set", Type = ItemType.Product, BuyingPrice = 300000, SellingPrice = 420000, StockQuantity = 4, LowStockThreshold = 2 }
            );
        }

        if (!db.Customers.Any())
        {
            db.Customers.Add(new Customer
            {
                Name = "Acme Industries Ltd.",
                BillingAddress = "123 Industrial Area, Dhaka",
                ContactPerson = "Mr. Rahman",
                Phone = "+880 1700 000000",
                Email = "info@acme.com",
                VatBin = "BIN-123456",
                Sites = new List<CustomerSite>
                {
                    new CustomerSite { SiteName = "Main Factory", Address = "Gazipur" }
                }
            });
        }

        db.SaveChanges();
    }
}
