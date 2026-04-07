using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyHub.API.DTOs;
using StudyHub.API.Services;

namespace StudyHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialsController : ControllerBase
{
    private readonly MaterialsService _materials;

    public MaterialsController(MaterialsService materials) => _materials = materials;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] MaterialsQueryDto query) =>
        Ok(await _materials.GetMaterialsAsync(query));

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await _materials.GetCategoriesAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var m = await _materials.GetByIdAsync(id);
        return m == null ? NotFound() : Ok(m);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateMaterialDto dto)
    {
        var created = await _materials.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateMaterialDto dto)
    {
        var updated = await _materials.UpdateAsync(id, dto);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _materials.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPatch("{id}/archive"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Archive(int id)
    {
        var updated = await _materials.SetArchivedAsync(id, true);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id}/restore"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int id)
    {
        var updated = await _materials.SetArchivedAsync(id, false);
        return updated == null ? NotFound() : Ok(updated);
    }
}
