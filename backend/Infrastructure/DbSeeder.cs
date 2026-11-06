using ReadyErp.Api.Domain;

namespace ReadyErp.Api.Infrastructure;

/// <summary>
/// Creates the schema and inserts demo data.
///
/// IMPORTANT: this uses <c>EnsureCreated()</c>, not migrations. It will not
/// alter an existing database — after any schema change you must delete
/// <c>readyerp.db</c> (plus the <c>-shm</c> / <c>-wal</c> files) and re-run.
/// </summary>
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

        SeedWarehouses(db);
        SeedCategories(db);
        db.SaveChanges();

        SeedProducts(db);
        SeedCustomers(db);

        db.SaveChanges();
    }

    private static void SeedWarehouses(AppDbContext db)
    {
        if (db.Warehouses.Any()) return;

        db.Warehouses.AddRange(
            new Warehouse
            {
                Code = "W1",
                Name = "Central Depot",
                Address = "Tejgaon Industrial Area, Dhaka",
                Locations =
                {
                    new Location { Code = "A-01", Description = "Heavy equipment bay" },
                    new Location { Code = "A-02", Description = "Genset staging" },
                    new Location { Code = "B-01", Description = "Controller shelving" },
                    new Location { Code = "B-02", Description = "Spare parts bins" }
                }
            },
            new Warehouse
            {
                Code = "W2",
                Name = "Chattogram Yard",
                Address = "Port Access Road, Chattogram",
                Locations =
                {
                    new Location { Code = "C-01", Description = "Inbound container unloading" },
                    new Location { Code = "C-02", Description = "Solar panel racking" }
                }
            });
    }

    private static void SeedCategories(AppDbContext db)
    {
        if (db.Categories.Any()) return;

        db.Categories.AddRange(
            new Category { Name = "Diesel Gensets", Description = "Standby and prime power generating sets" },
            new Category { Name = "Compressors", Description = "Screw and reciprocating air compressors" },
            new Category { Name = "Synchronization & Automation", Description = "Controllers, panels and automation gear" },
            new Category { Name = "Spare Parts & Service", Description = "Consumables, spares and labour" },
            new Category { Name = "Solar Power", Description = "On-grid and off-grid solar systems" });
    }

    private static void SeedProducts(AppDbContext db)
    {
        if (db.Products.Any()) return;

        var categories = db.Categories.ToDictionary(c => c.Name, c => c.Id);
        var warehouses = db.Warehouses.ToDictionary(w => w.Code, w => w.Id);
        var locations = db.Locations.ToDictionary(l => $"{l.WarehouseId}:{l.Code}", l => l.Id);

        int? Loc(string warehouseCode, string code) =>
            locations.TryGetValue($"{warehouses[warehouseCode]}:{code}", out var id) ? id : null;

        db.Products.AddRange(
            new Product
            {
                PartNumber = "DG-DEUTZ-100",
                Name = "DEUTZ Diesel Genset 100kVA",
                CategoryId = categories["Diesel Gensets"],
                Unit = "Pcs",
                Type = ItemType.Product,
                BuyingPrice = 1200000,
                SellingPrice = 1500000,
                StockQuantity = 5,
                LowStockThreshold = 2,
                WarehouseId = warehouses["W1"],
                LocationId = Loc("W1", "A-01")
            },
            new Product
            {
                PartNumber = "CMP-SCREW-50",
                Name = "Oil-injected Screw Compressor 50HP",
                CategoryId = categories["Compressors"],
                Unit = "Pcs",
                Type = ItemType.Product,
                BuyingPrice = 450000,
                SellingPrice = 600000,
                StockQuantity = 3,
                LowStockThreshold = 1,
                WarehouseId = warehouses["W1"],
                LocationId = Loc("W1", "A-02")
            },
            new Product
            {
                PartNumber = "CTRL-SMARTGEN",
                Name = "SmartGen Controller HGM6120",
                CategoryId = categories["Synchronization & Automation"],
                Unit = "Pcs",
                Type = ItemType.Product,
                BuyingPrice = 25000,
                SellingPrice = 38000,
                StockQuantity = 20,
                LowStockThreshold = 5,
                WarehouseId = warehouses["W1"],
                LocationId = Loc("W1", "B-01")
            },
            // Low stock, so the dashboard and status filters have a live example.
            new Product
            {
                PartNumber = "SP-FILTER-KIT",
                Name = "Genset Service Filter Kit",
                CategoryId = categories["Spare Parts & Service"],
                Unit = "Set",
                Type = ItemType.Product,
                BuyingPrice = 3500,
                SellingPrice = 6200,
                StockQuantity = 2,
                LowStockThreshold = 6,
                WarehouseId = warehouses["W1"],
                LocationId = Loc("W1", "B-02")
            },
            // Out of stock example.
            new Product
            {
                PartNumber = "SP-AVR-UNIVERSAL",
                Name = "Universal AVR Module",
                CategoryId = categories["Spare Parts & Service"],
                Unit = "Pcs",
                Type = ItemType.Product,
                BuyingPrice = 8000,
                SellingPrice = 13500,
                StockQuantity = 0,
                LowStockThreshold = 3,
                WarehouseId = warehouses["W1"],
                LocationId = Loc("W1", "B-02")
            },
            new Product
            {
                PartNumber = "SVC-INSTALL",
                Name = "Installation & Commissioning Service",
                CategoryId = categories["Spare Parts & Service"],
                Unit = "Job",
                Type = ItemType.Service,
                BuyingPrice = 0,
                SellingPrice = 50000
            },
            new Product
            {
                PartNumber = "SOLAR-ONGRID-5",
                Name = "On-grid Solar System 5kW",
                CategoryId = categories["Solar Power"],
                Unit = "Set",
                Type = ItemType.Product,
                BuyingPrice = 300000,
                SellingPrice = 420000,
                StockQuantity = 4,
                LowStockThreshold = 2,
                WarehouseId = warehouses["W2"],
                LocationId = Loc("W2", "C-02")
            });
    }

    private static void SeedCustomers(AppDbContext db)
    {
        if (db.Customers.Any()) return;

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
                new() { SiteName = "Main Factory", Address = "Gazipur" }
            }
        });
    }
}
