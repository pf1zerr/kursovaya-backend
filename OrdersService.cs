using Microsoft.EntityFrameworkCore;
using StudyHub.API.Data;
using StudyHub.API.DTOs;
using StudyHub.API.Models;

namespace StudyHub.API.Services;

public class OrdersService
{
    private readonly AppDbContext _db;

    public OrdersService(AppDbContext db) => _db = db;

    public async Task<(bool Success, string Message, OrderDto? Order)> PurchaseAsync(int userId, int materialId)
    {
        var material = await _db.Materials.FindAsync(materialId);
        if (material == null) return (false, "Material not found", null);

        var exists = await _db.Orders.AnyAsync(o => o.UserId == userId && o.MaterialId == materialId);
        if (exists) return (false, "Already purchased", null);

        var order = new Order { UserId = userId, MaterialId = materialId };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var dto = new OrderDto(
            order.Id,
            new MaterialDto(material.Id, material.Title, material.Description,
                material.Category, material.Price, material.FileUrl, material.CreatedAt, material.IsArchived),
            order.PurchasedAt
        );
        return (true, "Success", dto);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(int userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Material)
            .OrderByDescending(o => o.PurchasedAt)
            .Select(o => new OrderDto(
                o.Id,
                new MaterialDto(o.Material.Id, o.Material.Title, o.Material.Description,
                    o.Material.Category, o.Material.Price, o.Material.FileUrl, o.Material.CreatedAt, o.Material.IsArchived),
                o.PurchasedAt
            ))
            .ToListAsync();
    }

    public async Task<bool> HasAccessAsync(int userId, int materialId) =>
        await _db.Orders.AnyAsync(o => o.UserId == userId && o.MaterialId == materialId);
}
