using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadyErp.Api.Domain;
using ReadyErp.Api.Dtos;
using ReadyErp.Api.Infrastructure;

namespace ReadyErp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IEnumerable<CategoryDto>> GetAll([FromQuery] bool includeInactive = false)
    {
        var query = _db.Categories.AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description, c.IsActive,
                c.Products.Count(p => p.IsActive)))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var dto = await _db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description, c.IsActive,
                c.Products.Count(p => p.IsActive)))
            .FirstOrDefaultAsync();

        return dto is null ? NotFound() : dto;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsert dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name))
            return Conflict(new { message = $"Category '{dto.Name}' already exists." });

        var category = new Category { Name = dto.Name.Trim(), Description = dto.Description };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = category.Id },
            new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryUpsert dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        if (await _db.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id))
            return Conflict(new { message = $"Category '{dto.Name}' already exists." });

        category.Name = dto.Name.Trim();
        category.Description = dto.Description;
        await _db.SaveChangesAsync();

        var count = await _db.Products.CountAsync(p => p.CategoryId == id && p.IsActive);
        return new CategoryDto(category.Id, category.Name, category.Description, category.IsActive, count);
    }

    /// <summary>
    /// Soft-deletes the category. Products keep their reference; the category
    /// simply stops appearing in pickers.
    /// </summary>
    [HttpPost("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound();

        category.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
