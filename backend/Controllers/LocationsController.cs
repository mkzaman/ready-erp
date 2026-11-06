using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly AppDbContext _db;
    public LocationsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<LocationDto>> GetAll(
        [FromQuery] int? warehouseId,
        [FromQuery] bool includeInactive = false)
    {
        var query = _db.Locations.AsQueryable();
        if (warehouseId is not null) query = query.Where(l => l.WarehouseId == warehouseId);
        if (!includeInactive) query = query.Where(l => l.IsActive);

        return await query
            .OrderBy(l => l.Warehouse!.Code).ThenBy(l => l.Code)
            .Select(l => new LocationDto(
                l.Id, l.WarehouseId, l.Warehouse!.Name, l.Code, l.Description, l.IsActive))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LocationDto>> Get(int id)
    {
        var dto = await _db.Locations
            .Where(l => l.Id == id)
            .Select(l => new LocationDto(
                l.Id, l.WarehouseId, l.Warehouse!.Name, l.Code, l.Description, l.IsActive))
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : dto;
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(LocationUpsert dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { message = "Code is required." });

        var warehouse = await _db.Warehouses.FindAsync(dto.WarehouseId);
        if (warehouse is null)
            return BadRequest(new { message = "The selected warehouse no longer exists." });

        if (await _db.Locations.AnyAsync(l => l.WarehouseId == dto.WarehouseId && l.Code == dto.Code))
            return Conflict(new { message = $"Location '{dto.Code}' already exists in {warehouse.Name}." });

        var location = new Location
        {
            WarehouseId = dto.WarehouseId,
            Code = dto.Code.Trim(),
            Description = dto.Description
        };

        _db.Locations.Add(location);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = location.Id },
            new LocationDto(location.Id, location.WarehouseId, warehouse.Name, location.Code, location.Description, location.IsActive));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LocationDto>> Update(int id, LocationUpsert dto)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return NotFound();

        var warehouse = await _db.Warehouses.FindAsync(dto.WarehouseId);
        if (warehouse is null)
            return BadRequest(new { message = "The selected warehouse no longer exists." });

        if (await _db.Locations.AnyAsync(l => l.WarehouseId == dto.WarehouseId && l.Code == dto.Code && l.Id != id))
            return Conflict(new { message = $"Location '{dto.Code}' already exists in {warehouse.Name}." });

        location.WarehouseId = dto.WarehouseId;
        location.Code = dto.Code.Trim();
        location.Description = dto.Description;
        await _db.SaveChangesAsync();

        return new LocationDto(location.Id, location.WarehouseId, warehouse.Name, location.Code, location.Description, location.IsActive);
    }

    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var location = await _db.Locations.FindAsync(id);
        if (location is null) return NotFound();

        location.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
