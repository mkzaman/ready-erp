using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Services;

public class NumberingService
{
    private readonly AppDbContext _db;
    public NumberingService(AppDbContext db) => _db = db;

    // Single running sequence shared across all document types, matching EGEL practice.
    public string Next(string type)
    {
        var year = DateTime.UtcNow.Year;
        var counter = _db.DocumentCounters.FirstOrDefault(c => c.Year == year);
        if (counter is null)
        {
            counter = new Domain.DocumentCounter { Year = year, LastNumber = 708 };
            _db.DocumentCounters.Add(counter);
        }
        counter.LastNumber++;
        _db.SaveChanges();
        return $"EGEL/{type}/{year}/{counter.LastNumber}";
    }
}
