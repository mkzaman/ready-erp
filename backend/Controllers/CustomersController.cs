using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CustomersController(AppDbContext db) => _db = db;

    private static CustomerDto Map(Customer c) => new(
        c.Id, c.Name, c.BillingAddress, c.ContactPerson, c.Phone, c.Email, c.VatBin, c.IsActive,
        c.Sites.Select(s => new CustomerSiteDto(s.Id, s.SiteName, s.Address)).ToList());

    [HttpGet]
    public async Task<IEnumerable<CustomerDto>> GetAll([FromQuery] string? search)
    {
        var query = _db.Customers.Include(c => c.Sites).Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s));
        }
        return await query.OrderBy(c => c.Name).Select(c => Map(c)).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Get(int id)
    {
        var c = await _db.Customers.Include(x => x.Sites).FirstOrDefaultAsync(x => x.Id == id);
        return c is null ? NotFound() : Map(c);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerUpsert dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            BillingAddress = dto.BillingAddress,
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            VatBin = dto.VatBin,
            Sites = dto.Sites?.Select(s => new CustomerSite { SiteName = s.SiteName, Address = s.Address }).ToList() ?? new()
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = customer.Id }, Map(customer));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CustomerDto>> Update(int id, CustomerUpsert dto)
    {
        var customer = await _db.Customers.Include(c => c.Sites).FirstOrDefaultAsync(c => c.Id == id);
        if (customer is null) return NotFound();

        customer.Name = dto.Name;
        customer.BillingAddress = dto.BillingAddress;
        customer.ContactPerson = dto.ContactPerson;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.VatBin = dto.VatBin;

        _db.CustomerSites.RemoveRange(customer.Sites);
        customer.Sites = dto.Sites?.Select(s => new CustomerSite { SiteName = s.SiteName, Address = s.Address }).ToList() ?? new();

        await _db.SaveChangesAsync();
        return Map(customer);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null) return NotFound();
        customer.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
