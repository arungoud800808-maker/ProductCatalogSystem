using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Constants;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    // Anyone can view categories
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _service.GetAllAsync();
        return Ok(categories);
    }

    // Anyone can view category by Id
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound(new
            {
                Message = $"Category with Id {id} not found."
            });

        return Ok(category);
    }

    [Authorize(Policy = Permissions.CategoryCreate)]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        var category = await _service.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            category);
    }

    // Admin only
    [Authorize(Policy = Permissions.CategoryUpdate)]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, UpdateCategoryDto dto)
    {
        var category = await _service.UpdateAsync(id, dto);

        if (category == null)
            return NotFound(new
            {
                Message = $"Category with Id {id} not found."
            });

        return Ok(category);
    }

    // Admin only
    [Authorize(Policy = Permissions.CategoryDelete)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _service.GetByIdAsync(id);

        if (category == null)
            return NotFound(new
            {
                Message = $"Category with Id {id} not found."
            });

        await _service.DeleteAsync(id);

        return NoContent();
    }
}