using Microsoft.EntityFrameworkCore;
using StudyHub.API.Data;
using StudyHub.API.DTOs;
using StudyHub.API.Models;

namespace StudyHub.API.Services;

public class MaterialsService
{
    private readonly AppDbContext _db;

    public MaterialsService(AppDbContext db) => _db = db;

    public async Task<PagedResult<MaterialDto>> GetMaterialsAsync(MaterialsQueryDto query)
    {
        var q = _db.Materials.AsQueryable();

        if (!query.IncludeArchived)
            q = q.Where(m => !m.IsArchived);

        if (!string.IsNullOrWhiteSpace(query.Search))
            q = q.Where(m => m.Title.ToLower().Contains(query.Search.ToLower()));

        if (!string.IsNullOrWhiteSpace(query.Category))
            q = q.Where(m => m.Category == query.Category);

        if (query.MinPrice.HasValue)
            q = q.Where(m => m.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(m => m.Price <= query.MaxPrice.Value);

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(m => m.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => ToDto(m))
            .ToListAsync();

        return new PagedResult<MaterialDto>(items, total, query.Page, query.PageSize);
    }

    public async Task<MaterialDto?> GetByIdAsync(int id)
    {
        var m = await _db.Materials.FindAsync(id);
        return m == null ? null : ToDto(m);
    }

    public async Task<List<string>> GetCategoriesAsync() =>
        await _db.Materials.Where(m => !m.IsArchived).Select(m => m.Category).Distinct().ToListAsync();

    public async Task<MaterialDto> CreateAsync(CreateMaterialDto dto)
    {
        var m = new Material
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Price = dto.Price,
            FileUrl = dto.FileUrl
        };
        _db.Materials.Add(m);
        await _db.SaveChangesAsync();
        return ToDto(m);
    }

    public async Task<MaterialDto?> UpdateAsync(int id, UpdateMaterialDto dto)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return null;

        m.Title = dto.Title;
        m.Description = dto.Description;
        m.Category = dto.Category;
        m.Price = dto.Price;
        m.FileUrl = dto.FileUrl;

        await _db.SaveChangesAsync();
        return ToDto(m);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return false;
        _db.Materials.Remove(m);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<MaterialDto?> SetArchivedAsync(int id, bool isArchived)
    {
        var m = await _db.Materials.FindAsync(id);
        if (m == null) return null;
        m.IsArchived = isArchived;
        await _db.SaveChangesAsync();
        return ToDto(m);
    }

    private static MaterialDto ToDto(Material m) =>
        new(m.Id, m.Title, m.Description, m.Category, m.Price, m.FileUrl, m.CreatedAt, m.IsArchived);
}
