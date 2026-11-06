using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/warehouses")]
public class WarehousesController : ControllerBase
{
    private readonly AppDbContext _db;
    public WarehousesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<WarehouseDto>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.Warehouses.AsQueryable();
        if (!includeInactive) query = query.Where(w => w.IsActive);

        return await query
            .OrderBy(w => w.Code)
            .Select(w => new WarehouseDto(
                w.Id, w.Code, w.Name, w.Address, w.IsActive,
                w.Locations.Count(l => l.IsActive)))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> Get(int id)
    {
        var dto = await _db.Warehouses
            .Where(w => w.Id == id)
            .Select(w => new WarehouseDto(
                w.Id, w.Code, w.Name, w.Address, w.IsActive,
                w.Locations.Count(l => l.IsActive)))
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : dto;
    }

    /// <summary>Bins belonging to one warehouse, for dependent dropdowns.</summary>
    [HttpGet("{id:int}/locations")]
    public async Task<IEnumerable<LocationDto>> GetLocations(int id, [FromQuery] bool includeInactive = false)
    {
        var query = _db.Locations.Where(l => l.WarehouseId == id);
        if (!includeInactive) query = query.Where(l => l.IsActive);

        return await query
            .OrderBy(l => l.Code)
            .Select(l => new LocationDto(
                l.Id, l.WarehouseId, l.Warehouse!.Name, l.Code, l.Description, l.IsActive))
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(WarehouseUpsert dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Code and name are required." });

        if (await _db.Warehouses.AnyAsync(w => w.Code == dto.Code))
            return Conflict(new { message = $"Warehouse code '{dto.Code}' already exists." });

        var warehouse = new Warehouse
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Address = dto.Address
        };

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = warehouse.Id },
            new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Address, warehouse.IsActive, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<WarehouseDto>> Update(int id, WarehouseUpsert dto)
    {
        var warehouse = await _db.Warehouses.FindAsync(id);
        if (warehouse is null) return NotFound();

        if (await _db.Warehouses.AnyAsync(w => w.Code == dto.Code && w.Id != id))
            return Conflict(new { message = $"Warehouse code '{dto.Code}' already exists." });

        warehouse.Code = dto.Code.Trim();
        warehouse.Name = dto.Name.Trim();
        warehouse.Address = dto.Address;
        await _db.SaveChangesAsync();

        var count = await _db.Locations.CountAsync(l => l.WarehouseId == id && l.IsActive);
        return new WarehouseDto(warehouse.Id, warehouse.Code, warehouse.Name, warehouse.Address, warehouse.IsActive, count);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var warehouse = await _db.Warehouses.FindAsync(id);
        if (warehouse is null) return NotFound();

        warehouse.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
