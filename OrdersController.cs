using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.API.Services;

namespace StudyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrdersService _orders;

    public OrdersController(OrdersService orders) => _orders = orders;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{materialId}")]
    public async Task<IActionResult> Purchase(int materialId)
    {
        var (success, message, order) = await _orders.PurchaseAsync(UserId, materialId);
        if (!success) return BadRequest(new { message });
        return Ok(order);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy() =>
        Ok(await _orders.GetUserOrdersAsync(UserId));

    [HttpGet("has-access/{materialId}")]
    public async Task<IActionResult> HasAccess(int materialId) =>
        Ok(new { hasAccess = await _orders.HasAccessAsync(UserId, materialId) });
}
